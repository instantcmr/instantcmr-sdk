# instantCMR SDK and sample application

This repository is part of the [instantCMR API](https://docs.instantcmr.com) documentation. The project is built around .NET 8.0, it should run on Windows, Linux and macOS. If needed, you find the installer for your platform at this [url](https://dotnet.microsoft.com/en-us/download).

**Warning:** This code is not intended to use in production. Especially because it only uses very basic error handling that is not suitable for real life in production scenarios. 

The SDK consists of the following projects:

- **icmr.integration**: a library containing wrappers around the endpoints and the types needed for communicating with the instantCMR backend.
- **icmr.integration.sample**: a console application demonstrating the usage of the library.


### Getting Started

You can compile and run the application or simply look around to familiarize yourself with the concepts. However, before you can try it out in a real environment, our support team needs to provide the necessary settings (API key, secret, endpoints, etc.).

To run the sample console application, follow these steps:

1. Clone the repository

```bash
    git clone git@github.com:instantcmr/instantcmr-sdk.git
```

2. Navigate to the sample directory.

```bash
    cd instantcmr-sdk/icmr.integration.sample
```

3. Setup your configuration file.

Fill out [config.json](icmr.integration.sample/config.json) with the settings provided to you.

4. Build the application using the following command:

```bash
    dotnet build
```

5. Run the application

```bash
    dotnet run -- --help
```

### Example: following your fleet

As drivers send in documents and Hub users make modifications, your integration queue gradually fills with messages. These messages will remain there until you start downloading them using the API. The following command demonstrates this in practice:

```bash
> dotnet run -- receive
13:47:54.405 [:main-1] INFO IntegrationSample - starting up
13:47:54.427 [:main-1] INFO IntegrationSample - waiting for task to finish...
13:47:54.429 [:main-1] INFO IntegrationSample - will download to ~/projects/instantcmr-sdk/icmr.integration.sample/downloads/
13:47:54.433 [.NET TP Worker:pool-6] INFO IntegrationSample - press any key to stop
13:47:54.435 [.NET TP Worker:pool-4] INFO Receiver - starting up
13:47:54.436 [.NET TP Worker:pool-4] DEBUG Receiver - receiving from iepn 'updates'...
...
```

The application starts synchronizing changes to your `downloads` directory. Send some images using the instantCMR Android application, and you will see them arriving in this folder shortly. When you are finished, press any key to terminate the application.

### Going deeper

Use the help feature of the application to learn about the available commands. Read the API documentation and explore the source code of this repository to familiarize yourself with the instantCMR API.

Don't hesitate to ask questions; our support team is ready to help.
