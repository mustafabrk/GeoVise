using Microsoft.AspNetCore.Mvc;
using ModelClass.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Net;
using System.IdentityModel.Tokens.Jwt;

namespace WebApp.Controllers
{
    public class RequestController : Controller
    {
        private readonly string _apiBaseUrl;
        private string JwtToken => HttpContext.Session.GetString("tokenKey") ?? string.Empty;

        public RequestController(IConfiguration configuration)
        {
            _apiBaseUrl = configuration["Api:BaseUrl"]?.TrimEnd('/')
                ?? throw new InvalidOperationException("Set Api:BaseUrl in WebApp configuration.");
        }

        #region Liste ve Info Get İşlemleri
        public List<ToDo> GetList()
        {
            var client = new RestClient();
            var request = new RestRequest($"{_apiBaseUrl}/api/Home/Get");
            request.AddHeader("Authorization", "Bearer " + JwtToken);
            var response = client.Get(request);

            return JsonConvert.DeserializeObject<List<ToDo>>(response.Content);
        }
        public ToDo GetItemById(int id)
        {
            var client = new RestClient();
            var request = new RestRequest($"{_apiBaseUrl}/api/Home/GetById?id=" + id);
            request.AddHeader("Authorization", "Bearer " + JwtToken);
            var response = client.Get(request);

            return JsonConvert.DeserializeObject<ToDo>(response.Content);
        }
        public ToDo GetItemByName(string name)
        {
            var client = new RestClient();
            var request = new RestRequest($"{_apiBaseUrl}/api/Home/GetByName?name=" + name);
            request.AddHeader("Authorization", "Bearer " + JwtToken);
            var response = client.Get(request);

            return JsonConvert.DeserializeObject<ToDo>(response.Content);
        } //FONKSİYONU SEARCH OLARAK OLUŞTUR. JS TARAFINDAN GÖNDERİLEN PARAMETRELERLE ARA. 
        public ToDo GetItemByNumber(int number)
        {
            var client = new RestClient();
            var request = new RestRequest($"{_apiBaseUrl}/api/Home/GetByNumber?number=" + number);
            request.AddHeader("Authorization", "Bearer " + JwtToken);
            var response = client.Get(request);

            return JsonConvert.DeserializeObject<ToDo>(response.Content);
        }



        public BSTableResp<List<ToDo>> GetSortedList(int limit, int offset, string? sortOrder, string? sortField, string? search)   //BSTableReq<ToDo> req
        {
            var client = new RestClient();
            var request = new RestRequest($"{_apiBaseUrl}/api/Home/PagingDB?search=" + search + "&offset=" + offset + "&limit=" + limit, Method.Get);
            request.AddHeader("Authorization", "Bearer " + JwtToken);
            var response = client.Execute(request);

            return JsonConvert.DeserializeObject<BSTableResp<List<ToDo>>>(response.Content);
        }



        public City GetCityItemById(int id)
        {
            var client = new RestClient();
            var request = new RestRequest($"{_apiBaseUrl}/api/Home/GetCityById?id=" + id);
            request.AddHeader("Authorization", "Bearer " + JwtToken);
            var response = client.Get(request);

            return JsonConvert.DeserializeObject<City>(response.Content);
        }
        public County GetCountyItemById(int id)
        {
            var client = new RestClient();
            var request = new RestRequest($"{_apiBaseUrl}/api/Home/GetCountyById?id=" + id);
            request.AddHeader("Authorization", "Bearer " + JwtToken);
            var response = client.Get(request);

            return JsonConvert.DeserializeObject<County>(response.Content);
        }
        public Neighborhood GetNeighborhoodItemById(int id)
        {
            var client = new RestClient();
            var request = new RestRequest($"{_apiBaseUrl}/api/Home/GetNeighborhoodById?id=" + id);
            request.AddHeader("Authorization", "Bearer " + JwtToken);
            var response = client.Get(request);

            return JsonConvert.DeserializeObject<Neighborhood>(response.Content);
        }
        public Building GetBuildingItemById(int id)
        {
            var client = new RestClient();
            var request = new RestRequest($"{_apiBaseUrl}/api/Home/GetBuildingById?id=" + id);
            request.AddHeader("Authorization", "Bearer " + JwtToken);
            var response = client.Get(request);

            return JsonConvert.DeserializeObject<Building>(response.Content);
        }
        public Transformer GetTransformerItemById(int id)
        {
            var client = new RestClient();
            var request = new RestRequest($"{_apiBaseUrl}/api/Home/GetTransformerById?id=" + id);
            request.AddHeader("Authorization", "Bearer " + JwtToken);
            var response = client.Get(request);

            return JsonConvert.DeserializeObject<Transformer>(response.Content);
        }
        public Street GetStreetItemById(int id)
        {
            var client = new RestClient();
            var request = new RestRequest($"{_apiBaseUrl}/api/Home/GetStreetById?id=" + id);
            request.AddHeader("Authorization", "Bearer " + JwtToken);
            var response = client.Get(request);

            return JsonConvert.DeserializeObject<Street>(response.Content);
        }
        public Straight GetStraightItemById(int id)
        {
            var client = new RestClient();
            var request = new RestRequest($"{_apiBaseUrl}/api/Home/GetStraightById?id=" + id);
            request.AddHeader("Authorization", "Bearer " + JwtToken);
            var response = client.Get(request);

            return JsonConvert.DeserializeObject<Straight>(response.Content);
        }
        public Subscriber GetSubscriberItemById(int id)
        {
            var client = new RestClient();
            var request = new RestRequest($"{_apiBaseUrl}/api/Home/GetSubscriberById?id=" + id);
            request.AddHeader("Authorization", "Bearer " + JwtToken);
            var response = client.Get(request);

            return JsonConvert.DeserializeObject<Subscriber>(response.Content);
        }
        #endregion
        #region Liste ve Info Post-Put-Delete İşlemleri
        public bool PostCity(City param)
        {
            bool result = false;

            try
            {
                using (var client = new RestClient($"{_apiBaseUrl}/api/Home/PostCity"))
                {
                    var payload = new JObject();

                    payload["number"] = param.Number;
                    payload["name"] = param.Name;
                    payload["coordinate"] = param.Coordinate;
                    payload["measurement"] = param.Measurement;
                    payload["featureType"] = param.FeatureType;
                    payload["locatedCity"] = param.LocatedCity;
                    payload["licensePlate"] = param.LicensePlate;
                    payload["population"] = param.Population;

                    var request = new RestRequest();
                    request.AddHeader("Authorization", "Bearer " + JwtToken);
                    request.AddStringBody(payload.ToString(), DataFormat.Json);

                    var response = client.Post(request);

                    result = true;
                    //result = JsonConvert.DeserializeObject<bool>(response.Content);
                }
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }
        public bool PostCounty(County param)
        {
            bool result = false;

            try
            {
                using (var client = new RestClient($"{_apiBaseUrl}/api/Home/PostCounty"))
                {
                    var payload = new JObject();

                    payload["number"] = param.Number;
                    payload["name"] = param.Name;
                    payload["coordinate"] = param.Coordinate;
                    payload["measurement"] = param.Measurement;
                    payload["featureType"] = param.FeatureType;
                    payload["locatedCity"] = param.LocatedCity;
                    payload["population"] = param.Population;

                    var request = new RestRequest();
                    request.AddHeader("Authorization", "Bearer " + JwtToken);
                    request.AddStringBody(payload.ToString(), DataFormat.Json);

                    var response = client.Post(request);

                    result = true;
                    //result = JsonConvert.DeserializeObject<bool>(response.Content);
                }
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }
        public bool PostNeighborhood(Neighborhood param)
        {
            bool result = false;

            try
            {
                using (var client = new RestClient($"{_apiBaseUrl}/api/Home/PostNeighborhood"))
                {
                    var payload = new JObject();

                    payload["number"] = param.Number;
                    payload["name"] = param.Name;
                    payload["coordinate"] = param.Coordinate;
                    payload["measurement"] = param.Measurement;
                    payload["featureType"] = param.FeatureType;
                    payload["locatedCity"] = param.LocatedCity;
                    payload["locatedCounty"] = param.LocatedCounty;
                    payload["population"] = param.Population;

                    var request = new RestRequest();
                    request.AddHeader("Authorization", "Bearer " + JwtToken);
                    request.AddStringBody(payload.ToString(), DataFormat.Json);

                    var response = client.Post(request);

                    result = true;
                    //result = JsonConvert.DeserializeObject<bool>(response.Content);
                }
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }
        public bool PostBuilding(Building param)
        {
            bool result = false;

            try
            {
                using (var client = new RestClient($"{_apiBaseUrl}/api/Home/PostBuilding"))
                {
                    var payload = new JObject();

                    payload["number"] = param.Number;
                    payload["name"] = param.Name;
                    payload["coordinate"] = param.Coordinate;
                    payload["measurement"] = param.Measurement;
                    payload["featureType"] = param.FeatureType;
                    payload["locatedCity"] = param.LocatedCity;
                    payload["buildingNo"] = param.BuildingNo;
                    payload["floornum"] = param.FloorNum;

                    var request = new RestRequest();
                    request.AddHeader("Authorization", "Bearer " + JwtToken);
                    request.AddStringBody(payload.ToString(), DataFormat.Json);

                    var response = client.Post(request);

                    result = true;
                    //result = JsonConvert.DeserializeObject<bool>(response.Content);
                }
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }
        public bool PostTransformer(Transformer param)
        {
            bool result = false;

            try
            {
                using (var client = new RestClient($"{_apiBaseUrl}/api/Home/PostTransformer"))
                {
                    var payload = new JObject();

                    payload["number"] = param.Number;
                    payload["name"] = param.Name;
                    payload["coordinate"] = param.Coordinate;
                    payload["measurement"] = param.Measurement;
                    payload["featureType"] = param.FeatureType;
                    payload["capacity"] = param.Capacity;
                    payload["locatedCity"] = param.LocatedCity;
                    payload["locatedCounty"] = param.LocatedCounty;
                    payload["locatedNeighborhood"] = param.LocatedNeighborhood;

                    var request = new RestRequest();
                    request.AddHeader("Authorization", "Bearer " + JwtToken);
                    request.AddStringBody(payload.ToString(), DataFormat.Json);

                    var response = client.Post(request);

                    result = true;
                    //result = JsonConvert.DeserializeObject<bool>(response.Content);
                }
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }
        public bool PostStreet(Street param)
        {
            bool result = false;

            try
            {
                using (var client = new RestClient($"{_apiBaseUrl}/api/Home/PostStreet"))
                {
                    var payload = new JObject();

                    payload["number"] = param.Number;
                    payload["name"] = param.Name;
                    payload["coordinate"] = param.Coordinate;
                    payload["measurement"] = param.Measurement;
                    payload["featureType"] = param.FeatureType;
                    payload["locatedCity"] = param.LocatedCity;
                    payload["locatedCounty"] = param.LocatedCounty;
                    payload["locatedNeighborhood"] = param.LocatedNeighborhood;

                    var request = new RestRequest();
                    request.AddHeader("Authorization", "Bearer " + JwtToken);
                    request.AddStringBody(payload.ToString(), DataFormat.Json);

                    var response = client.Post(request);

                    result = true;
                    //result = JsonConvert.DeserializeObject<bool>(response.Content);
                }
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }
        public bool PostStraight(Straight param)
        {
            bool result = false;

            try
            {
                using (var client = new RestClient($"{_apiBaseUrl}/api/Home/PostStraight"))
                {
                    var payload = new JObject();

                    payload["number"] = param.Number;
                    payload["name"] = param.Name;
                    payload["coordinate"] = param.Coordinate;
                    payload["measurement"] = param.Measurement;
                    payload["featureType"] = param.FeatureType;
                    payload["locatedCity"] = param.LocatedCity;
                    payload["straightType"] = param.StraightType;

                    var request = new RestRequest();
                    request.AddHeader("Authorization", "Bearer " + JwtToken);
                    request.AddStringBody(payload.ToString(), DataFormat.Json);

                    var response = client.Post(request);

                    result = true;
                    //result = JsonConvert.DeserializeObject<bool>(response.Content);
                }
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }
        public bool PostSubscriber(Subscriber param)
        {
            bool result = false;

            try
            {
                using (var client = new RestClient($"{_apiBaseUrl}/api/Home/PostSubscriber"))
                {
                    var payload = new JObject();

                    payload["number"] = param.Number;
                    payload["name"] = param.Name;
                    payload["coordinate"] = param.Coordinate;
                    payload["measurement"] = param.Measurement;
                    payload["featureType"] = param.FeatureType;
                    payload["locatedCity"] = param.LocatedCity;
                    payload["locatedCounty"] = param.LocatedCounty;
                    payload["locatedNeighborhood"] = param.LocatedNeighborhood;

                    var request = new RestRequest();
                    request.AddHeader("Authorization", "Bearer " + JwtToken);
                    request.AddStringBody(payload.ToString(), DataFormat.Json);

                    var response = client.Post(request);

                    result = true;
                    //result = JsonConvert.DeserializeObject<bool>(response.Content);
                }
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }



        public bool PutLine(ToDo doIt)
        {
            bool result = false;

            try
            {
                using (var client = new RestClient($"{_apiBaseUrl}/api/Home/"))
                {
                    var request = new RestRequest("Put?id=" + doIt.Id, Method.Put);
                    request.AddHeader("Authorization", "Bearer " + JwtToken);
                    request.AddJsonBody(doIt);

                    var response = client.Execute(request);

                    result = response.IsSuccessful;
                }
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }
        public bool PutCity(City param)
        {
            bool result = false;

            try
            {
                using (var client = new RestClient($"{_apiBaseUrl}/api/Home/"))
                {
                    var request = new RestRequest("PutCity?id=" + param.Id, Method.Put);
                    request.AddHeader("Authorization", "Bearer " + JwtToken);
                    request.AddJsonBody(param);

                    var response = client.Execute(request);

                    result = response.IsSuccessful;
                }
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }
        public bool PutCounty(County param)
        {
            bool result = false;

            try
            {
                using (var client = new RestClient($"{_apiBaseUrl}/api/Home/"))
                {
                    var request = new RestRequest("PutCounty?id=" + param.Id, Method.Put);
                    request.AddHeader("Authorization", "Bearer " + JwtToken);
                    request.AddJsonBody(param);

                    var response = client.Execute(request);

                    result = response.IsSuccessful;
                }
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }
        public bool PutNeighborhood(Neighborhood param)
        {
            bool result = false;

            try
            {
                using (var client = new RestClient($"{_apiBaseUrl}/api/Home/"))
                {
                    var request = new RestRequest("PutNeighborhood?id=" + param.Id, Method.Put);
                    request.AddHeader("Authorization", "Bearer " + JwtToken);
                    request.AddJsonBody(param);

                    var response = client.Execute(request);

                    result = response.IsSuccessful;
                }
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }
        public bool PutBuilding(Building param)
        {
            bool result = false;

            try
            {
                using (var client = new RestClient($"{_apiBaseUrl}/api/Home/"))
                {
                    var request = new RestRequest("PutBuilding?id=" + param.Id, Method.Put);
                    request.AddHeader("Authorization", "Bearer " + JwtToken);
                    request.AddJsonBody(param);

                    var response = client.Execute(request);

                    result = response.IsSuccessful;
                }
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }
        public bool PutTransformer(Transformer param)
        {
            bool result = false;

            try
            {
                using (var client = new RestClient($"{_apiBaseUrl}/api/Home/"))
                {
                    var request = new RestRequest("PutTransformer?id=" + param.Id, Method.Put);
                    request.AddHeader("Authorization", "Bearer " + JwtToken);
                    request.AddJsonBody(param);

                    var response = client.Execute(request);

                    result = response.IsSuccessful;
                }
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }
        public bool PutStreet(Street param)
        {
            bool result = false;

            try
            {
                using (var client = new RestClient($"{_apiBaseUrl}/api/Home/"))
                {
                    var request = new RestRequest("PutStreet?id=" + param.Id, Method.Put);
                    request.AddHeader("Authorization", "Bearer " + JwtToken);
                    request.AddJsonBody(param);

                    var response = client.Execute(request);

                    result = true;
                }
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }
        public bool PutStraight(Straight param)
        {
            bool result = false;

            try
            {
                using (var client = new RestClient($"{_apiBaseUrl}/api/Home/"))
                {
                    var request = new RestRequest("PutStraight?id=" + param.Id, Method.Put);
                    request.AddHeader("Authorization", "Bearer " + JwtToken);
                    request.AddJsonBody(param);

                    var response = client.Execute(request);

                    result = response.IsSuccessful;
                }
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }
        public bool PutSubscriber(Subscriber param)
        {
            bool result = false;

            try
            {
                using (var client = new RestClient($"{_apiBaseUrl}/api/Home/"))
                {
                    var request = new RestRequest("PutSubscriber?id=" + param.Id, Method.Put);
                    request.AddHeader("Authorization", "Bearer " + JwtToken);
                    request.AddJsonBody(param);

                    var response = client.Execute(request);

                    result = response.IsSuccessful;
                }
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }



        public bool DeleteLine(int id, string featureType)
        {
            bool result = false;

            try
            {
                var client = new RestClient($"{_apiBaseUrl}/api/Home/Delete");
                var request = new RestRequest($"{_apiBaseUrl}/api/Home/Delete?id=" + id + "&featureType=" + featureType, Method.Delete);
                request.AddHeader("Authorization", "Bearer " + JwtToken);

                var response = client.Delete(request);
                result = JsonConvert.DeserializeObject<bool>(response.Content);
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }
        #endregion
        #region Katman İşlemleri
        public List<string> GetLayerList(int id)
        {
            var client = new RestClient();
            var request = new RestRequest($"{_apiBaseUrl}/api/Home/GetLayerList?id=" + id);
            request.AddHeader("Authorization", "Bearer " + JwtToken);

            var response = client.Get(request);

            return JsonConvert.DeserializeObject<List<string>>(response.Content);
        }
        public List<Layers> GetLayerParams(int id)
        {
            var client = new RestClient();
            var request = new RestRequest($"{_apiBaseUrl}/api/Home/GetLayerParams?id=" + id);
            request.AddHeader("Authorization", "Bearer " + JwtToken);
            var response = client.Get(request);

            return JsonConvert.DeserializeObject<List<Layers>>(response.Content);
        }
        public bool ChangeLayerVisible(List<Layers> uls)
        {
            bool result = false;

            try
            {
                using (var client = new RestClient($"{_apiBaseUrl}/api/Home/"))
                {
                    var request = new RestRequest("ChangeLayerVisible", Method.Patch);
                    request.AddHeader("Authorization", "Bearer " + JwtToken);
                    request.AddJsonBody(uls);

                    var response = client.Execute(request);

                    result = response.IsSuccessful;
                }
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }
        public bool DeleteOldLayerSettings(int id)
        {
            bool result = false;

            try
            {
                using (var client = new RestClient($"{_apiBaseUrl}/api/Home/"))
                {
                    var request = new RestRequest("DeleteOldLayerSettings?id=" + id, Method.Delete);
                    request.AddHeader("Authorization", "Bearer " + JwtToken);

                    var response = client.Execute(request);

                    result = response.IsSuccessful;
                }
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }
        public bool PostNewLayerSettings(List<Layers> layers)
        {
            bool result = false;

            try
            {
                using (var client = new RestClient($"{_apiBaseUrl}/api/Home/"))
                {
                    var request = new RestRequest("PostNewLayerSettings", Method.Post);
                    request.AddHeader("Authorization", "Bearer " + JwtToken);
                    request.AddJsonBody(layers);

                    var response = client.Execute(request);

                    result = response.IsSuccessful;
                    //result = JsonConvert.DeserializeObject<bool>(response.Content);
                }
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }
        #endregion
        #region Kullanıcı Girişi ve Haritadan Seçme İşlemleri
        public List<MapInfo> GetInfoFromMap(string wkt, string layers, double zoomLevel)
        {
            var client = new RestClient();
            var request = new RestRequest($"{_apiBaseUrl}/api/Home/GetInfoMap?wkt=" + wkt + "&layers=" + layers + "&zoomLevel=" + zoomLevel);
            request.AddHeader("Authorization", "Bearer " + JwtToken);
            var response = client.Get(request);

            return JsonConvert.DeserializeObject<List<MapInfo>>(response.Content);
        }



        [HttpPost]
        public async Task<IActionResult> ControlLogin([FromForm] string username, [FromForm] string password, CancellationToken cancellationToken)
        {
            using var client = new RestClient($"{_apiBaseUrl}/api/Token/");
            var request = new RestRequest("Login", Method.Post)
                .AddJsonBody(new { UserName = username, Password = password });
            var response = await client.ExecuteAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                HttpContext.Session.Remove("User");
                HttpContext.Session.Remove("tokenKey");
                return Unauthorized(new { message = "Username or password is incorrect." });
            }

            if (response.ResponseStatus != ResponseStatus.Completed || !response.IsSuccessful || string.IsNullOrWhiteSpace(response.Content))
            {
                return StatusCode(StatusCodes.Status502BadGateway, new { message = response.ErrorMessage ?? "The authentication API did not return a successful response." });
            }

            var token = JsonConvert.DeserializeObject<string>(response.Content);
            if (string.IsNullOrWhiteSpace(token))
            {
                return StatusCode(StatusCodes.Status502BadGateway, new { message = "The authentication API returned an invalid token response." });
            }

            var user = GetUserFromToken(token);
            if (user is null)
            {
                return StatusCode(StatusCodes.Status502BadGateway, new { message = "The authentication API returned a token without user identity claims." });
            }

            HttpContext.Session.SetString("tokenKey", token);
            HttpContext.Session.SetString("User", JsonConvert.SerializeObject(user));
            return Ok(user);
        }

        private User? GetUserFromToken(string token)
        {
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;
            var userNameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserName")?.Value;

            if (!decimal.TryParse(userIdClaim, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var userId)
                || string.IsNullOrWhiteSpace(userNameClaim))
            {
                return null;
            }

            return new User
            {
                Id = userId,
                UserName = userNameClaim,
                SessionDuration = jwtToken.ValidTo
            };
        }

        [HttpPost]
        public async Task<IActionResult> RefreshToken(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(JwtToken))
            {
                return Unauthorized();
            }

            using var client = new RestClient($"{_apiBaseUrl}/api/Token/");
            var request = new RestRequest("Refresh", Method.Get)
                .AddHeader("Authorization", "Bearer " + JwtToken);
            var response = await client.ExecuteAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                HttpContext.Session.Remove("User");
                HttpContext.Session.Remove("tokenKey");
                return Unauthorized();
            }

            if (response.ResponseStatus != ResponseStatus.Completed || !response.IsSuccessful || string.IsNullOrWhiteSpace(response.Content))
            {
                return StatusCode(StatusCodes.Status502BadGateway, new { message = response.ErrorMessage ?? "The authentication API did not return a successful response." });
            }

            var token = JsonConvert.DeserializeObject<string>(response.Content);
            if (string.IsNullOrWhiteSpace(token))
            {
                return StatusCode(StatusCodes.Status502BadGateway, new { message = "The authentication API returned an invalid token response." });
            }

            HttpContext.Session.SetString("tokenKey", token);
            return Ok(new { token, expiresAt = new JwtSecurityTokenHandler().ReadJwtToken(token).ValidTo });
        }

        private List<Layers> UserLayersContent(int id)
        {
            List<Layers> layers = new List<Layers>();

            var client = new RestClient();
            var request = new RestRequest($"{_apiBaseUrl}/api/Home/GetUserLayerSettings?id=" + id);
            request.AddHeader("Authorization", "Bearer " + JwtToken);
            var response = client.Get(request);

            layers = JsonConvert.DeserializeObject<List<Layers>>(response.Content);

            HttpContext.Session.SetString("Layers", JsonConvert.SerializeObject(layers));
            RedirectToAction("LayersSideBar", layers);

            return layers;
        }
        private DateTime GetTokenExpiration(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

            if (jwtToken != null)
            {
                return jwtToken.ValidTo;
            }

            return DateTime.MinValue;
        }
        #endregion
    }
}