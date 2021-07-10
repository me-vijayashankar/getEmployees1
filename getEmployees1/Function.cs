using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using getEmployees;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace getEmployees1
{
    public class Function
    {
        public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine("executing API gateway");
            context.Logger.Log(JsonConvert.SerializeObject(request));
            if (request.Resource == "/employee")
            {
                var employeeProvider = new EmployeeProvider(new AmazonDynamoDBClient(), context);
                if (request.QueryStringParameters != null)
                {
                    var location = request.QueryStringParameters["location"];
                    context.Logger.Log($"location: {location}");
                    if(location!=null && location != "")
                    {
                        var employeesByLoc = await employeeProvider.GetEmployeeByLocationAsync(location);
                        return new APIGatewayProxyResponse { StatusCode = (int)System.Net.HttpStatusCode.OK, Body = JsonConvert.SerializeObject(employeesByLoc) };
                    }
                }
                
                var employees = await employeeProvider.GetEmployeesAsync();
                return new APIGatewayProxyResponse() { StatusCode = (int)System.Net.HttpStatusCode.OK, Body = JsonConvert.SerializeObject(employees) };
            }
            else if (request.Resource == "/employee/{id}")
            {
                if(request.PathParameters["id"] != null && request.PathParameters["id"] != "")
                {
                    var employeeProvider = new EmployeeProvider(new AmazonDynamoDBClient(), context);
                    var employee = await employeeProvider.GetEmployeeAsync(request.PathParameters["id"]);
                    if (employee != null)
                        return new APIGatewayProxyResponse { StatusCode = (int)System.Net.HttpStatusCode.OK, Body = JsonConvert.SerializeObject(employee) };
                    else
                        return new APIGatewayProxyResponse { StatusCode = (int)System.Net.HttpStatusCode.NotFound, Body = $"Not found for {request.PathParameters["id"]} " };
                }
                else return new APIGatewayProxyResponse { StatusCode = (int)System.Net.HttpStatusCode.BadRequest };
            }
            return new APIGatewayProxyResponse { StatusCode = (int)System.Net.HttpStatusCode.BadRequest };

        }
    }
}
