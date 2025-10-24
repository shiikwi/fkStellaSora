1.Find the original filename of the `.bin` you want to convert.(try hex editor)  
2.Write the corresponding Protobuf (`.proto`) definition for the data structure. Refer to [Achievement.proto](https://github.com/shiikwi/fkStellaSora/blob/main/protobuf/Achievement.proto), or just use [YostarGames.cs](https://github.com/Hiro420/StellaSoraParser/blob/main/StellaSoraParser/YostarGames.cs)  
3.Generate the C# class and change parameters in Program.cs.
```bash
protoc.exe --csharp_out=. xxx.proto
```