using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using GS1.Core.DTOs;

namespace GS1.WinClient.Services;


/// API Client servisi - Backend ile iletişim

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    #region Customer Operations

    public async Task<ApiResponse<IEnumerable<CustomerDto>>?> GetCustomersAsync()
    {
        return await GetAsync<IEnumerable<CustomerDto>>("api/customers");
    }

    public async Task<ApiResponse<CustomerDto>?> GetCustomerAsync(Guid id)
    {
        return await GetAsync<CustomerDto>($"api/customers/{id}");
    }

    public async Task<ApiResponse<CustomerWithProductsDto>?> GetCustomerWithProductsAsync(Guid id)
    {
        return await GetAsync<CustomerWithProductsDto>($"api/customers/{id}/with-products");
    }

    public async Task<ApiResponse<CustomerDto>?> CreateCustomerAsync(CustomerCreateDto dto)
    {
        return await PostAsync<CustomerDto>("api/customers", dto);
    }

    public async Task<ApiResponse<CustomerDto>?> UpdateCustomerAsync(Guid id, CustomerUpdateDto dto)
    {
        return await PutAsync<CustomerDto>($"api/customers/{id}", dto);
    }

    public async Task<ApiResponse<bool>?> DeleteCustomerAsync(Guid id)
    {
        return await DeleteAsync<bool>($"api/customers/{id}");
    }

    #endregion

    #region Product Operations

    public async Task<ApiResponse<IEnumerable<ProductDto>>?> GetProductsAsync()
    {
        return await GetAsync<IEnumerable<ProductDto>>("api/products");
    }

    public async Task<ApiResponse<ProductDto>?> GetProductAsync(Guid id)
    {
        return await GetAsync<ProductDto>($"api/products/{id}");
    }

    public async Task<ApiResponse<IEnumerable<ProductDto>>?> GetProductsByCustomerAsync(Guid customerId)
    {
        return await GetAsync<IEnumerable<ProductDto>>($"api/products/by-customer/{customerId}");
    }

    public async Task<ApiResponse<ProductDto>?> CreateProductAsync(ProductCreateDto dto)
    {
        return await PostAsync<ProductDto>("api/products", dto);
    }

    public async Task<ApiResponse<ProductDto>?> UpdateProductAsync(Guid id, ProductUpdateDto dto)
    {
        return await PutAsync<ProductDto>($"api/products/{id}", dto);
    }

    public async Task<ApiResponse<bool>?> DeleteProductAsync(Guid id)
    {
        return await DeleteAsync<bool>($"api/products/{id}");
    }

    #endregion

    #region WorkOrder Operations

    public async Task<ApiResponse<IEnumerable<WorkOrderDto>>?> GetWorkOrdersAsync()
    {
        return await GetAsync<IEnumerable<WorkOrderDto>>("api/workorders");
    }

    public async Task<ApiResponse<WorkOrderDto>?> GetWorkOrderAsync(Guid id)
    {
        return await GetAsync<WorkOrderDto>($"api/workorders/{id}");
    }

    public async Task<ApiResponse<WorkOrderDetailDto>?> GetWorkOrderDetailsAsync(Guid id)
    {
        return await GetAsync<WorkOrderDetailDto>($"api/workorders/{id}/details");
    }

    public async Task<ApiResponse<WorkOrderDto>?> CreateWorkOrderAsync(WorkOrderCreateDto dto)
    {
        return await PostAsync<WorkOrderDto>("api/workorders", dto);
    }

    public async Task<ApiResponse<WorkOrderDto>?> UpdateWorkOrderAsync(Guid id, WorkOrderUpdateDto dto)
    {
        return await PutAsync<WorkOrderDto>($"api/workorders/{id}", dto);
    }

    public async Task<ApiResponse<IEnumerable<SerialNumberDto>>?> GenerateSerialsAsync(Guid workOrderId, int quantity)
    {
        return await PostAsync<IEnumerable<SerialNumberDto>>($"api/workorders/{workOrderId}/generate-serials", quantity);
    }

    public async Task<ApiResponse<bool>?> DeleteWorkOrderAsync(Guid id)
    {
        return await DeleteAsync<bool>($"api/workorders/{id}");
    }

    #endregion

    #region SerialNumber Operations

    public async Task<ApiResponse<IEnumerable<SerialNumberDto>>?> GetSerialNumbersByWorkOrderAsync(Guid workOrderId)
    {
        return await GetAsync<IEnumerable<SerialNumberDto>>($"api/serialnumbers/by-workorder/{workOrderId}");
    }

    public async Task<ApiResponse<IEnumerable<SerialNumberDto>>?> GetUnassignedSerialsAsync(Guid workOrderId)
    {
        return await GetAsync<IEnumerable<SerialNumberDto>>($"api/serialnumbers/unassigned/{workOrderId}");
    }

    #endregion

    #region Aggregation Operations

    public async Task<ApiResponse<IEnumerable<SSCCDto>>?> GetSSCCsByWorkOrderAsync(Guid workOrderId)
    {
        return await GetAsync<IEnumerable<SSCCDto>>($"api/aggregation/by-workorder/{workOrderId}");
    }

    public async Task<ApiResponse<IEnumerable<SSCCDto>>?> GetBoxesAsync(Guid workOrderId)
    {
        return await GetAsync<IEnumerable<SSCCDto>>($"api/aggregation/boxes/{workOrderId}");
    }

    public async Task<ApiResponse<IEnumerable<SSCCDto>>?> GetPalletsAsync(Guid workOrderId)
    {
        return await GetAsync<IEnumerable<SSCCDto>>($"api/aggregation/pallets/{workOrderId}");
    }

    public async Task<ApiResponse<SSCCAggregationDto>?> GetAggregationHierarchyAsync(Guid ssccId)
    {
        return await GetAsync<SSCCAggregationDto>($"api/aggregation/{ssccId}/hierarchy");
    }

    public async Task<ApiResponse<SSCCDto>?> CreateBoxAsync(Guid workOrderId)
    {
        var dto = new SSCCCreateDto(workOrderId, GS1.Core.Entities.SSCCType.Box);
        return await PostAsync<SSCCDto>("api/aggregation/create-box", dto);
    }

    public async Task<ApiResponse<SSCCDto>?> CreatePalletAsync(Guid workOrderId)
    {
        var dto = new SSCCCreateDto(workOrderId, GS1.Core.Entities.SSCCType.Pallet);
        return await PostAsync<SSCCDto>("api/aggregation/create-pallet", dto);
    }

    public async Task<ApiResponse<SSCCAggregationDto>?> AddItemsToBoxAsync(Guid boxId, List<Guid> serialNumberIds)
    {
        var dto = new AddItemsToBoxDto(boxId, serialNumberIds);
        return await PostAsync<SSCCAggregationDto>("api/aggregation/add-items-to-box", dto);
    }

    public async Task<ApiResponse<SSCCAggregationDto>?> AddBoxesToPalletAsync(Guid palletId, List<Guid> boxIds)
    {
        var dto = new AddBoxesToPalletDto(palletId, boxIds);
        return await PostAsync<SSCCAggregationDto>("api/aggregation/add-boxes-to-pallet", dto);
    }

    public async Task<ApiResponse<bool>?> RemoveItemFromBoxAsync(Guid serialNumberId)
    {
        return await DeleteAsync<bool>($"api/aggregation/remove-item/{serialNumberId}");
    }

    public async Task<ApiResponse<bool>?> RemoveBoxFromPalletAsync(Guid boxId)
    {
        return await DeleteAsync<bool>($"api/aggregation/remove-box/{boxId}");
    }

    #endregion

    #region HTTP Helper Methods

    private async Task<ApiResponse<T>?> GetAsync<T>(string endpoint)
    {
        try
        {
            var response = await _httpClient.GetAsync(endpoint);
            return await DeserializeResponse<T>(response);
        }
        catch (Exception ex)
        {
            return ApiResponse<T>.ErrorResponse($"Bağlantı hatası: {ex.Message}");
        }
    }

    private async Task<ApiResponse<T>?> PostAsync<T>(string endpoint, object? data)
    {
        try
        {
            var content = data != null 
                ? new StringContent(JsonSerializer.Serialize(data, _jsonOptions), Encoding.UTF8, "application/json")
                : null;
            
            var response = await _httpClient.PostAsync(endpoint, content);
            return await DeserializeResponse<T>(response);
        }
        catch (Exception ex)
        {
            return ApiResponse<T>.ErrorResponse($"Bağlantı hatası: {ex.Message}");
        }
    }

    private async Task<ApiResponse<T>?> PutAsync<T>(string endpoint, object data)
    {
        try
        {
            var content = new StringContent(JsonSerializer.Serialize(data, _jsonOptions), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(endpoint, content);
            return await DeserializeResponse<T>(response);
        }
        catch (Exception ex)
        {
            return ApiResponse<T>.ErrorResponse($"Bağlantı hatası: {ex.Message}");
        }
    }

    private async Task<ApiResponse<T>?> PatchAsync<T>(string endpoint, object? data)
    {
        try
        {
            var content = data != null
                ? new StringContent(JsonSerializer.Serialize(data, _jsonOptions), Encoding.UTF8, "application/json")
                : new StringContent("{}", Encoding.UTF8, "application/json");
            
            var request = new HttpRequestMessage(HttpMethod.Patch, endpoint) { Content = content };
            var response = await _httpClient.SendAsync(request);
            return await DeserializeResponse<T>(response);
        }
        catch (Exception ex)
        {
            return ApiResponse<T>.ErrorResponse($"Bağlantı hatası: {ex.Message}");
        }
    }

    private async Task<ApiResponse<T>?> DeleteAsync<T>(string endpoint)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(endpoint);
            return await DeserializeResponse<T>(response);
        }
        catch (Exception ex)
        {
            return ApiResponse<T>.ErrorResponse($"Bağlantı hatası: {ex.Message}");
        }
    }

    private async Task<ApiResponse<T>?> DeserializeResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        
        if (string.IsNullOrEmpty(content))
            return ApiResponse<T>.ErrorResponse("Boş yanıt alındı");

        try
        {
            return JsonSerializer.Deserialize<ApiResponse<T>>(content, _jsonOptions);
        }
        catch
        {
            return ApiResponse<T>.ErrorResponse($"Yanıt işlenemedi: {content}");
        }
    }

    #endregion
}
