# WebApi.FileUploader

## Overview
WebApi.FileUploader is a robust file management system designed with modern .NET technologies. It provides file upload capabilities, cloud storage integration, image processing, distributed caching, background job processing, and comprehensive API documentation.

## Project Structure
The solution contains the following projects:

- **WebApi.FileUploader**: Main API project responsible for handling file uploads, integrations, and core functionalities.
- **HashGenerator**: Utility project for hash generation, used for file integrity and security.
- **WebApi.FileUploader.Tests**: Test project for unit testing and validation of core functionalities.

## Features

### File Management
- File upload capabilities.
- Integration with AWS S3 for cloud storage.
- Image processing and manipulation using SixLabors.ImageSharp.

### Caching System
- Distributed caching with Redis.
- Performance optimization using StackExchange.Redis.

### Background Processing
- Job scheduling and background task processing with Hangfire.
- Memory storage for job data using Hangfire.MemoryStorage.

### Data Persistence
- MongoDB integration for document storage using MongoDB.Driver.

### API Documentation
- Interactive API documentation through Swagger (Swashbuckle.AspNetCore).
- Built-in API testing interface.

### Hash Generation
- Hash generation functionality for ensuring file integrity or security.

### Testing
- Unit testing with the dedicated `WebApi.FileUploader.Tests` project.

## Technologies & Frameworks

### Core Framework
- **.NET 8.0**: Latest version of the .NET framework.

### Cloud Storage
- **AWS S3 Integration**: Using AWSSDK.S3 v3.7.304.10.

### Caching
- **Redis Cache**: StackExchange.Redis v2.8.24.
- **Microsoft.Extensions.Caching.StackExchangeRedis**: v9.0.1.

### Database
- **MongoDB**: MongoDB.Driver v2.13.3.

### Background Job Processing
- **Hangfire**: v1.8.17.
- **Hangfire.MemoryStorage**: v1.8.1.1.

### Image Processing
- **SixLabors.ImageSharp**: v3.1.5 for image manipulation.

### API Documentation
- **Swagger/OpenAPI**: Swashbuckle.AspNetCore v6.4.0.

### JSON Processing
- **Newtonsoft.Json**: v13.0.3 for advanced JSON processing.

## Development Environment
- **IDE**: Visual Studio 2022.
- **HTTPS Port**: Development runs on port `52682`.
- **Nullable Reference Types**: Enabled.
- **Implicit Using Statements**: Enabled.

## Setup and Installation

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or later
- AWS account with S3 bucket configured
- Redis server
- MongoDB instance

### Installation Steps
1. Clone the repository:
   ```bash
   git clone https://github.com/your-repo/WebApi.FileUploader.git
   ```
2. Navigate to the solution directory:
   ```bash
   cd WebApi.FileUploader
   ```
3. Restore dependencies:
   ```bash
   dotnet restore
   ```
4. Update the `appsettings.json` with your configurations for:
   - AWS S3
   - Redis connection string
   - MongoDB connection string
5. Run the project:
   ```bash
   dotnet run --project WebApi.FileUploader
   ```
6. Access the API documentation at:
   ```
   https://localhost:52682/swagger
   ```

## Usage
- **File Upload**: Upload files to the API, which are then stored in AWS S3.
- **Image Processing**: Perform operations like resizing or format conversion.
- **Hash Generation**: Generate hashes for files to ensure data integrity.
- **Background Tasks**: Use Hangfire to schedule and monitor jobs.
- **Cache Management**: Leverage Redis for optimized performance.

## Testing
To run unit tests:
```bash
dotnet test
```

## Contribution Guidelines
We welcome contributions! Please follow these steps:
1. Fork the repository.
2. Create a feature branch:
   ```bash
   git checkout -b feature-name
   ```
3. Commit your changes:
   ```bash
   git commit -m "Description of changes"
   ```
4. Push to your fork:
   ```bash
   git push origin feature-name
   ```
5. Open a pull request.

## License
This project is licensed under the MIT License. See the LICENSE file for details.

## Contact
For any queries or issues, please open an issue in the repository or contact [tumitaa@hotmail.com].

