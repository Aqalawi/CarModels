Car Models API
This ASP.NET Core Web API provides an endpoint to retrieve car models for a specific make and manufacture year using data from an external API.
Instructions to Start the Application Locally
**Prerequisites**
.NET Core SDK installed on your machine
**Steps**
1.Clone the repository to your local machine:
-git clone https://github.com/your-username/car-models-api.git
-cd car-models-api
2.Open a terminal or command prompt and navigate to the project directory:
-cd CarModelsApi
3.Build the application using the following command:
-dotnet build
4.Run the application with the following command:
-dotnet run
**The application will start and be accessible at http://localhost:5000 by default.**
5.Open your favorite API testing tool (e.g., Postman) and send a GET request to retrieve car models. For example:

Request:GET http://localhost:5000/api/models?modelyear=2015&make=Lincoln
Response(json):
{
  "Models": ["Town Car", "Continental", "Mark"]
}
6.To stop the application, press Ctrl+C in the terminal where the application is running.
