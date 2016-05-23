# SQL Proxy / Replay: For database-isolated testing

This library came into being because I wanted to be able to test performance changes to a long-running service. There were two classes of change that I wanted to make - firstly, identifying hot paths (or particularly inefficient areas of code) that could be tweaked to be faster and make a significant impact on CPU usage. Secondly, I wanted to investigate ways to reduce the load on the garbage collector - most likely through reducing allocations - and to be able to measure improvements over time.

The nice thing about a UI-less data service is that it can be easy to simulate load - incoming requests can be captured and serialised so that they can later be played back to an installation of the service that is only responding to those replayed queries. The problem comes when the service needs to make calls to external systems for data, in my case that meant a SQL database. And, which makes things easier for me, a SQL database that is only being read from.

One obvious way to try to deal with this is to have a database available specifically for running the service against, in a reliable consistent state so that the data that is returned for the same queries is identical each and every time. This is a reasonable solution to the problem, requiring only a little organisation in ensuring that there are appropriate database(s) available and that they are not used for anything else (particularly that they are not used by anything that might try to change the data).

I wanted to try something different that I thought might work for me - if I'm capturing requests on the way into the service, so that I can replay them to simulate load without actually having any real clients generating that load, can't I capture the SQL requests (and the data they result in) somewhere and then have that data played back when I do my test runs, rather than requiring a real database? (This would have to be a remote service since I don't want to add any additional load to the data service that I'm trying to optimise).

## The client code

This is how the SQL access commonly looks -

	var sql = @"
	  SELECT productId, productName
	  FROM Products
	  WHERE ProductName LIKE '%' + @name + '%'";
	
	var products = new List<Product>();
	using (var conn = new SqlConnection(connectionString))
	{
	  conn.Open();
	  using (var cmd = new SqlCommand(sql, conn))
	  {
	    cmd.Parameters.AddWithValue("name", "Bob");
	    using (var rdr = cmd.ExecuteReader())
	    {
	      while (rdr.Read())
	      {
	        products.Add(new Product
	        {
	          ProductId = rdr.GetInt32(rdr.GetOrdinal("productId")),
	          ProductName = rdr.GetString(rdr.GetOrdinal("productName"))
	        });		  
	      }
	    }
	  }
	}

If you're making life easier on yourself and using a micro-ORM\* then it might look something more like this:

	var sql = @"
	  SELECT productId, productName
	  FROM Products
	  WHERE ProductName LIKE '%' + @name + '%'";
	
	List<Product> products;
	using (var conn = new SqlConnection(connectionString))
	{
	  conn.Open();
      products = conn.Query<Product>(sql, new { name = "Bob" }).ToList();
	}
	
\* *("full fat ORMs" are so divisive that I am reticent to mention them - but something similar is, of course, possible)*

When using this library, the code would need to be changed slightly to -

	var sql = @"
	  SELECT productId, productName
	  FROM Products
	  WHERE ProductName LIKE '%' + @name + '%'";
	
	var products = new List<Product>();
	using (var conn = new RemoteSqlClient(connectionString, proxyEndPoint))
	{
	  conn.Open();
	  using (var cmd = conn.CreateCommand(sql))
	  {
	    cmd.Parameters.AddWithValue("name", "Bob");
	    using (var rdr = cmd.ExecuteReader())
	    {
	      while (rdr.Read())
	      {
	        products.Add(new Product
	        {
	          ProductId = rdr.GetInt32(rdr.GetOrdinal("productId")),
	          ProductName = rdr.GetString(rdr.GetOrdinal("productName"))
	        });		  
	      }
	    }
	  }
	}
	
or:

	var sql = @"
	  SELECT productId, productName
	  FROM Products
	  WHERE ProductName LIKE '%' + @name + '%'";
	
	List<Product> products;
	using (var conn = new RemoteSqlClient(connectionString, proxyEndPoint))
	{
	  conn.Open();
      products = conn.Query<Product>(sql, new { name = "Bob" }).ToList();
	}
	
Fairly minor changes (though potentially inconvenient if there are many, many places that you are creating connections and queries - in my case, there are only between ten and twenty places that need this change to be applied).

The "RemoteSqlClient" class fully implements the "IDbConnection" interface but sends all queries and property accesses over to a WCF service (using the binary "NetTcpBinding"). This means that there is unavoidable overhead to using this proxy when the queries are being forward on to a real database - but I'm interested in how the service performs when it actually has the data, I'm not interested (at this time) in how efficient its SQL queries are.

The WCF service may be configured in one of two ways - either as "pass-through proxy" or as a "replayer". When it's a proxy, commands sent to it are passed to the database. First, though, the service will execute the query in order to cache the entire data set that the query generates and *then* execute it a second separate time in order to feed the data back to the client. When the service is configured as a replayer, it will never talk to a database, it will only return cached results for known queries (throwing exceptions for any queries whose results are not known).

## The server / host code

The server is simple to set up - the "Host" class will listen for client requests and takes an "ISqlProxy" implementation as a constructor argument. The "SqlProxy" class will record queries and their results:

	var cache = new DiskCache(
	  SqlRunner.Instance,
	  cacheFolder: new DirectoryInfo("Cache"),
	  infoLogger: Console.WriteLine
	);
	using (var proxyHost = new Host(
	  new SqlProxy(
	    () => new SqlConnection(),
	    cache.QueryRecorder,
	    cache.ScalarQueryRecorder,
	    cache.NonQueryRowCountRecorder
	  ),
	  proxyEndPoint
	))
	{
	  Console.WriteLine("Proxy listening for connections..");
	  Console.WriteLine("Press [Enter] to terminate..");
	  Console.ReadLine();
	}

To configure a service to replay cached data, the "SqlReplayer" class should be used to initialise the "Host" class (instead of using a SqlProxy instance) -

	var cache = new DiskCache(
	  SqlRunner.Instance,
	  cacheFolder: new DirectoryInfo("Cache"),
	  infoLogger: Console.WriteLine
	);
	using (var proxyHost = new Host(
	  new SqlReplayer(
	    cache.DataRetriever,
	    cache.ScalarDataRetriever,
	    cache.NonQueryRowCountRetriever
	  ),
	  replayEndPoint
	))
	{
	  Console.WriteLine("Replayer listening for connections..");
	  Console.WriteLine("Press [Enter] to terminate..");
	  Console.ReadLine();
	}

## In use

In case it's not apparent by this point, the general plan with this service is to do the following:

1. Generate requests for the service that is to be tested, either based upon real interactions with the system (by capturing requests while the system is in use) or by creating synthetic requests to exercise particular areas of the system
2. Run these requests against an instance of the service that is set up for testing - have the service point to a **proxy** host so that all SQL queries and result sets are captured
3. Run the requests again against an instance of the service under test, with the service pointing to a **replayer** host - this will allow you to establish a base line for performance with the database out of equation and with consistent results guaranteed to come back from SQL queries every time
4. Make any performance changes and re-test, still pointing at a **replayer** host - start measuring any changes (hopefully improvements!)

## Limitations and compromises

Again, this proxy / replayer service will be useful for me in testing my particular service - the most obvious simplification that I'm able to make is that it is primarily a read-only system and so database writes are not required. Since the proxy / replayer cache records a single result set for every distinct combination of connection string / command / parameter(s), it would not be possible to simulate an environment for a service that expects to execute a query once, then perform an update, then execute that same query again and get back different data. The proxy / replayer *does* support "ExecuteNonQuery" (which is often used for update statements) and it will cache the return value from such a call (which will be [the number of rows affected](https://msdn.microsoft.com/en-us/library/system.data.sqlclient.sqlcommand.executenonquery(v=vs.110).aspx)), which might be enough for you to use it with a system that *does* read and write - but the limitation remains that it's not possible for the replayer to return different result sets at different times for any single connection string / command / parameter(s) combination.

When the "SqlProxy" class is instantiated on the server, one of the arguments is a "connection generator", which determines how a new connection is created. In the example code above (and the primary use case that I have in mind), this will be a SqlConnection - but that doesn't mean that *only* SqlConnection is supported. For example, in the integration tests, I use an in-memory Sqlite database so that the tests have as few external dependencies to configure as possible. If you wanted to use this with a different database provider, then you would need only to change the "connection generator" that your proxy service used.

If this seems like it might be useful to you, then the final hurdle will be changing your code to call the proxy / replayer service while under test. In my case, while the project itself is complex, the places that it interacts with a database are relatively few - maybe a dozen or so. I intend to change these such that the database-calling classes take a new "connection generator" dependency; a delegate that will return a new SqlConnection instance in normal use.. or a proxy connection to either a proxy or replayer endpoint while in testing. These changes should be fairly painless and not require any significant coupling of the service-under-test code to the proxy-replayer-service-client code.

As a final reminder, I am making no promises about performance about the proxy / replayer service. It seems likely that a proxy endpoint will be slower than direct SQL access because there is an indirection involved (instead of talking to the database, your code talks to the proxy service which then talks to the database) *and* the query is executed twice (once to cache and once to feed back to the caller). It also seems likely that a replayer endpoint will be faster than direct SQL access, since there will be no table joins or complicated lookups and filtering to perform - the results are essentially maintained in a denormalised cache. Neither of these scenarios should be relevant, though, since the point of leaning on the proxy / replayer service will be to measure the performance *elsewhere* in the system, without any possible variance in the data that the database may return. As is always the case with performance investigations, you should be measuring at all points - and should be looking for changes *away* from the database access in the sort of cases where this service might be helpful. There are loads of great tools for databases for improving query execution time, the aim of this service is to make it easier to dig into the *rest* of the potential bottlenecks!