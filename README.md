# Persian UI Controls Maui

Persian Calendar &amp; some other controls for .NET MAUI

To use this package in your MAUI project use the below code in your MauiProgram.cs file




## Deployment

To deploy this project run

```bash
  public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                    {
                        fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                        fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    })
                .UseMauiCommunityToolkit()
                .UsePersianUIControls();
            return builder.Build();
        }
    }
```


## Screenshots

![App Screenshot](https://raw.githubusercontent.com/RezaShaban/DesignPatterns/67627cea2105fdbf301fcd722bc73444535ef42f/DesignPatternTutorial/Screenshot%202023-06-05%20230750.png)

