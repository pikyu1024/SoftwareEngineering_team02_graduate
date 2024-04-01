# ProjectManageSystemBackend

## Project Build

安裝visual studio 2022插件要裝.net 跟 asp.net

<img src="https://github.com/112598028/SoftwareEngineering_team02/blob/main/readmejpg/3.jpg" align="center" height="350" width="600"/>

開啟112-course-project-team-02-main\backend\project-manage-system-backend 

<img src="https://github.com/112598028/SoftwareEngineering_team02/blob/main/readmejpg/4.jpg" align="center" height="350" width="800"/>

並點擊

<img src="https://github.com/112598028/SoftwareEngineering_team02/blob/main/readmejpg/5.jpg" align="center" height="350" width="600"/>

開啟成功

<img src="https://github.com/112598028/SoftwareEngineering_team02/blob/main/readmejpg/6.jpg" align="center" height="350" width="600"/>



EF Core
```
dotnet tool install --global dotnet-ef --version 5.0.11 //安裝EFCORE TOOL
dotnet ef migrations add InitialCreate //當有變動到DB MODEL時需執行
dotnet ef database update //更新資料庫
```
[參考資料](https://docs.microsoft.com/en-us/ef/core/get-started/overview/first-app?tabs=netcore-cli)


## 後端測試環境設置 (backend/PMS_test/PMS_test.csproj)

安裝 EntityFrameworkCore 8

<img src="https://github.com/112598028/SoftwareEngineering_team02/blob/main/readmejpg/11.PNG" align="center" height="350" width="1200"/>


