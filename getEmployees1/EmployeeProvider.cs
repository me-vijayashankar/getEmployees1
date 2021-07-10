using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

namespace getEmployees
{
    public class EmployeeProvider : IEmployeeProvider
    {
        private readonly IAmazonDynamoDB dynamoDB;
        private readonly ILambdaContext context;

        public EmployeeProvider(IAmazonDynamoDB dynamoDB, ILambdaContext context)
        {
            this.dynamoDB = dynamoDB;
            this.context = context;
        }

        public async Task<Employee[]> GetEmployeesAsync()
        {
            var result = await dynamoDB.ScanAsync(new ScanRequest
            {
                TableName = "Employee"
            });
            if (result != null && result.Items != null)
            {
                var employees = new List<Employee>();
                foreach (var item in result.Items)
                {
                    item.TryGetValue("Emp_ID", out var EmpID);
                    item.TryGetValue("Emp_name", out var EmpName);
                    item.TryGetValue("Emp_location", out var EmpLocation);
                    employees.Add(new Employee { EmpID = EmpID.S, EmpName = EmpName.S, EmpLocation = EmpLocation.S });

                }
                return employees.ToArray();
            }
            return Array.Empty<Employee>();
        }

        public async Task<Employee> GetEmployeeAsync(string id)
        {
            context.Logger.Log($"id : {id}");

            var request = new QueryRequest
            {
                TableName = "Employee",
                KeyConditionExpression = "Emp_ID = :v_Id",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                    {":v_Id", new AttributeValue { S = id }}
                }
            };
            var response = await dynamoDB.QueryAsync(request);

            if(response != null && response.Items != null && response.Count>0)
            {
                response.Items[0].TryGetValue("Emp_ID", out var EmpID);
                response.Items[0].TryGetValue("Emp_name", out var EmpName);
                response.Items[0].TryGetValue("Emp_location", out var EmpLocation);
                var employee = new Employee
                {
                    EmpID = EmpID.S,
                    EmpName = EmpName.S,
                    EmpLocation = EmpLocation.S
                };
                return employee;
            }
            context.Logger.Log(JsonConvert.SerializeObject(response));

            return null;
        }

        public async Task<Employee[]> GetEmployeeByLocationAsync(string location)
        {
            context.Logger.Log($"id : {location}");

            ScanRequest request = new ScanRequest
            {
                TableName = "Employee",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":loc", new AttributeValue { S = location } }
                },
                FilterExpression = "Emp_location = :loc"
            };

            var response = await dynamoDB.ScanAsync(request);

            if (response != null && response.Items != null && response.Count > 0)
            {
                List<Employee> empResult = new List<Employee>();
                foreach (var item in response.Items)
                {
                    item.TryGetValue("Emp_ID", out var EmpID);
                    item.TryGetValue("Emp_name", out var EmpName);
                    item.TryGetValue("Emp_location", out var EmpLocation);
                    var employee = new Employee
                    {
                        EmpID = EmpID.S,
                        EmpName = EmpName.S,
                        EmpLocation = EmpLocation.S
                    };
                    empResult.Add(employee);
                }
                context.Logger.Log(JsonConvert.SerializeObject(empResult));
                return empResult.ToArray();
            }
            context.Logger.Log(JsonConvert.SerializeObject(response));

            return null;
        }
    }
}
