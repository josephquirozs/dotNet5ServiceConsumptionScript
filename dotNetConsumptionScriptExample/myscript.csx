#r "nuget: Newtonsoft.Json, 13.0.1"
#r "nuget: Microsoft.AspNet.WebApi.Client, 5.2.7"
#r "nuget: Stubble.Core, 1.9.3"

using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Stubble.Core;
using Stubble.Core.Builders;

Console.WriteLine("Running script...");

// Configuring HTTP Client
HttpClient client = new HttpClient();
client.BaseAddress = new Uri(uriString: "http://192.168.1.57:5000/api/");
client.DefaultRequestHeaders.Accept.Clear();
client.DefaultRequestHeaders.Accept.Add(item: new MediaTypeWithQualityHeaderValue("application/json"));

// Initialize service
ProductService productService = new ProductService(client: client);

// Search resources
await productService.GetAll(searchValue: "");

// Retrieve a resource
// await productService.GetOne(productId: 1);

// Create new resource
// await productService.Post(new Product()
// {
//     ProductId = null,
//     Name = "IPHONE 13",
//     Price = 999.99m,
//     Stock = 99.0m,
//     Unit = "UND",
//     Expiration = DateTime.Now,
// });

// Update resource
// await productService.Put(new Product()
// {
//     ProductId = 1,
//     Name = "IPHONE 13 (EDITADO 1)",
//     Price = 999.99m,
//     Stock = 0.0m,
//     Unit = "UND",
//     Expiration = DateTime.Now,
// });

// Delete resource
// await productService.Delete(productId: 101);

// Start of Product.cs
public partial class Product
{
    [JsonProperty("productId", NullValueHandling = NullValueHandling.Ignore)]
    public long? ProductId { get; set; }

    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; set; }

    [JsonProperty("price", NullValueHandling = NullValueHandling.Ignore)]
    public decimal? Price { get; set; }

    [JsonProperty("stock", NullValueHandling = NullValueHandling.Ignore)]
    public decimal? Stock { get; set; }

    [JsonProperty("unit", NullValueHandling = NullValueHandling.Ignore)]
    public string Unit { get; set; }

    [JsonProperty("expiration", NullValueHandling = NullValueHandling.Ignore)]
    public DateTime? Expiration { get; set; }
}

public partial class Product
{
    public static Product FromJson(string json) => JsonConvert.DeserializeObject<Product>(json, Converter.Settings);
}

// This will not work!
// public static class Serialize
// {
//     public static string ToJson(this Product self) => JsonConvert.SerializeObject(self, Converter.Settings);
// }

// Must be declared as an extension method at the top level
public static string ToJson(this Product self) => JsonConvert.SerializeObject(self, Converter.Settings);

internal static class Converter
{
    public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
    {
        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
        DateParseHandling = DateParseHandling.None,
        Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeLocal }
            },
    };
}
// End of Product.cs

// Start of ProductEndpoints.cs
public static class ProductEndpoints
{
    public const string GetAll = @"products";
    public const string GetOne = @"products/{{productId}}";
    public const string Post = @"products";
    public const string Put = @"products";
    public const string Delete = @"products/{{productId}}";
}
// End of ProductEndpoints.cs

// Start of ProductSegments.cs
public static class ProductSegments
{
    public const string ProductId = @"productId";
}
// End of ProductSegments.cs

// Start of ProductParameters.cs
public static class ProductParameters
{
    public const string PageNumber = @"pageNumber";
    public const string PageSize = @"pageSize";
    public const string SearchValue = @"searchValue";
}
// End of ProductParameters.cs

// Start of ProductService.cs
public class ProductService
{
    private readonly HttpClient _client;

    public ProductService(HttpClient client)
    {
        _client = client;
    }

    public async Task<IEnumerable<Product>> GetAll(int pageNumber = 1, int pageSize = 10, string searchValue = "")
    {
        if (searchValue == null)
        {
            throw new ArgumentException("Search value must not be null");
        }
        string fSearchValue = System.Net.WebUtility.UrlEncode(searchValue.Trim());
        string endpointUri = RenderEndpointUri(method: HttpMethod.Get, endpoint: ProductEndpoints.GetAll);
        endpointUri += $"?{ProductParameters.PageNumber}={pageNumber}";
        endpointUri += $"&{ProductParameters.PageSize}={pageSize}";
        endpointUri += $"&{ProductParameters.SearchValue}={fSearchValue}";
        string requestUrl = $"{_client.BaseAddress}{endpointUri}";
        Console.WriteLine($"Request GET to {requestUrl}");
        HttpResponseMessage response = await _client.GetAsync(requestUri: endpointUri);
        int responseStatus = (int)response.StatusCode;
        string responseBody = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Response status {responseStatus}");
        Console.WriteLine($"Response body {responseBody}");
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("An error occurred on the server side");
        }
        return await response.Content.ReadAsAsync<IEnumerable<Product>>();
    }

    public async Task<Product> GetOne(long productId)
    {
        Dictionary<string, string> segmentValues = new Dictionary<string, string>() {
            { ProductSegments.ProductId, productId.ToString() },
        };
        string endpointUri = RenderEndpointUri(method: HttpMethod.Get, endpoint: ProductEndpoints.GetOne, segmentValues: segmentValues);
        string requestUrl = $"{_client.BaseAddress}{endpointUri}";
        Console.WriteLine($"Request GET to {requestUrl}");
        HttpResponseMessage response = await _client.GetAsync(requestUri: endpointUri);
        int responseStatus = (int)response.StatusCode;
        string responseBody = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Response status {responseStatus}");
        Console.WriteLine($"Response body {responseBody}");
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("An error occurred on the server side");
        }
        return await response.Content.ReadAsAsync<Product>();
    }

    public async Task<Product> Post(Product resource)
    {
        string endpointUri = RenderEndpointUri(method: HttpMethod.Post, endpoint: ProductEndpoints.Post);
        string requestUrl = $"{_client.BaseAddress}{endpointUri}";
        string requestBody = resource.ToJson();
        Console.WriteLine($"Request POST to {requestUrl}");
        Console.WriteLine($"Request body {requestBody}");
        HttpResponseMessage response = await _client.PostAsJsonAsync<Product>(requestUri: endpointUri, value: resource);
        int responseStatus = (int)response.StatusCode;
        string responseBody = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Response status {responseStatus}");
        Console.WriteLine($"Response body {responseBody}");
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("An error occurred on the server side");
        }
        return await response.Content.ReadAsAsync<Product>();
    }

    public async Task<Product> Put(Product resource)
    {
        string endpointUri = RenderEndpointUri(method: HttpMethod.Put, endpoint: ProductEndpoints.Put);
        string requestUrl = $"{_client.BaseAddress}{endpointUri}";
        string requestBody = resource.ToJson();
        Console.WriteLine($"Request PUT to {requestUrl}");
        Console.WriteLine($"Request body {requestBody}");
        HttpResponseMessage response = await _client.PutAsJsonAsync<Product>(requestUri: endpointUri, value: resource);
        int responseStatus = (int)response.StatusCode;
        string responseBody = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Response status {responseStatus}");
        Console.WriteLine($"Response body {responseBody}");
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("An error occurred on the server side");
        }
        return await response.Content.ReadAsAsync<Product>();
    }

    public async Task Delete(long productId)
    {
        Dictionary<string, string> segmentValues = new Dictionary<string, string>() {
            { ProductSegments.ProductId, productId.ToString() },
        };
        string endpointUri = RenderEndpointUri(method: HttpMethod.Delete, endpoint: ProductEndpoints.Delete, segmentValues: segmentValues);
        string requestUrl = $"{_client.BaseAddress}{endpointUri}";
        Console.WriteLine($"Request DELETE to {requestUrl}");
        HttpResponseMessage response = await _client.DeleteAsync(requestUri: endpointUri);
        int responseStatus = (int)response.StatusCode;
        string responseBody = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Response status {responseStatus}");
        Console.WriteLine($"Response body {responseBody}");
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("An error occurred on the server side");
        }
    }

    private string RenderEndpointUri(HttpMethod method, string endpoint, Dictionary<string, string> segmentValues = null)
    {
        switch (method.Method)
        {
            case "GET":
                switch (endpoint)
                {
                    case ProductEndpoints.GetAll:
                        return endpoint;
                    case ProductEndpoints.GetOne:
                        return RenderTemplate(template: endpoint, values: segmentValues);
                    default:
                        throw new NotImplementedException("GET method not implemented");
                }
            case "POST":
            case "PUT":
                return endpoint;
            case "DELETE":
                return RenderTemplate(template: endpoint, values: segmentValues);
            default:
                throw new NotImplementedException("HTTP method not implemented");
        }
    }

    private string RenderTemplate(string template, Dictionary<string, string> values)
    {
        if (values == null || values.Count == 0)
        {
            throw new ArgumentException("Template values must not be null or empty");
        }
        StubbleVisitorRenderer stubble = new StubbleBuilder().Build();
        return stubble.Render(template: template, view: values);
    }
}
// End of ProductService.cs