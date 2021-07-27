# MapCsharp
Map engine for displaing tiles and routes. Based on C#, WPF

Enter ./GridTestApplication


![image](https://user-images.githubusercontent.com/50050208/127168505-0d395649-8f8d-4aa3-8167-579c2feafecd.png)

### You can set a track. For this do 
`map.AddTrack(trackPoints2, "Путь")`

1 param - `Dictionary <DateTime, Point>` points dictionary
; 2 param - `string` track name

### Either icon via: 
![image](https://user-images.githubusercontent.com/50050208/127169399-9ed35b07-0444-415f-832e-21cb1deab43d.png)

`map.AddIcon(new Point(37.6173, 55.7558),"car", "0");`
