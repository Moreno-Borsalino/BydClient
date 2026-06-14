# BydClient - C\# client for the BYD vehicle API.
The goal is to access the BYD vehicle APIs to obtain information on the car's operating status and send commands in the future.

# **📖 Overview**

This project is a C\# port of the PHP project https://github.com/VitalyArt/byd-php-client
The goal is to access the BYD vehicle APIs to obtain information about the car's operating status and send commands in the future.  
This is a first version and needs to be tested with access to various car models to ensure the porting has been done without errors.  
I'm counting on the help of some BYD owners and C\# developers to test the access and report any errors.  
You can examine the code to ensure there's no risk of credential theft.  
This is purely an exercise of curiosity and to fill the gap in the C\# version for accessing the BYD APIs. In fact, there are only PHP or Python projects.  
If the information reading part of the code works, you can already see the parts for sending commands to the car, and this will be the next step in a future version.

Thanks to anyone who wants to try it and provide me with results and suggestions.

## Features

- [x] Authentication
- [x] Vehicle listing
- [x] Real-time data
- [x] GPS information
- [x] HVAC status
- [x] Charging status
- [x] Energy consumption
- [x] Remote control
- [x] Smart charging
- [x] Push notifications
- [x] Vehicle settings

## Configuration

The client can be configured in several ways:

### appsettings.json file
If this file is present, the information it contains is used. If it is absent, environment variables can be used otherwise default inside source code is used.

If BaseUrl is empty, it is calculated based on the CountryCode field. You can view the CSV files that calculate the URLs to use based on the selected country.
```json
{
  "BYD": {
    "Username": "test@example.com",
    "Password": "password123",
    "BaseUrl": "https://dilinkappoversea-eu.byd.auto",
    "CountryCode": "NL",
    "Language": "en"
  }
}
```

> [!TIP]
> Use a dedicated BYD account for the integration, that way you won't be logged out from the BYD app on your main account - see [here](https://www.youtube.com/watch?v=DRzsjYHjlqQ) for instructions.


### Environment Variables

```env
BYD_USERNAME=your-email@example.com
BYD_PASSWORD=your-password
BYD_BASE_URL=https://dilinkappoversea-eu.byd.auto
BYD_COUNTRY_CODE=NL
BYD_LANGUAGE=en
BYD_TIME_ZONE=Europe/Amsterdam
```

## **🚀 Getting Started**

To run this solution locally, ensure you have the **.NET SDK** installed or load it into **Visual Studio**. This solution contains a ***BydClient*** project, which is a Console Application. You can examine the Program.cs file to see what information is requested from the vehicle. Or you can modify it to suit your needs.

### **Prerequisites**

* [.NET SDK](https://dotnet.microsoft.com/download) (Version 10.0 or higher recommended)  
* Visual Studio or VS Code

### **Installation**

1. **Clone the repository:**  
   git clone https://github.com/Moreno-Borsalino/BydClient.git

2. **Navigate to the project directory:**  
   cd BydClient\BydClient

3. **edit appsettings.json file:**  
   In appsettings.json file write the information about:  
    **Username e Password** for authentication  
    **CountryCode** to automatically select the server URL for your country  
    **Language** to have messages in your preferred language  
    **BaseUrl** set value in this field if you want use a specific BYD server

     **Example:**
   If you want a configuration for Germany with german messages use this file:
```json
{
  "BYD": {
    "Username": "test@example.com",
    "Password": "password123",
    "BaseUrl": "",
    "CountryCode": "DE",
    "Language": "de"
  }
}
```

5. **Build and Run:**  
   dotnet run



## **👨‍💻 Author**

**Moreno Borsalino**

* **Role:** Passionate Developer  
* **Focus:** .NET Backend-End Development & MAUI App

*If you find this repository helpful or interesting, please give it a ⭐️\!*

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
