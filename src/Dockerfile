# ---- Stage 1: Build ----
# 使用 .NET SDK 镜像
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# 1. 复制 .sln 和所有 .csproj 文件
#    使用您 `ls` 命令确认的、确切的大小写文件名，避免使用通配符 '*'
COPY ["cocity.sln", "."]
COPY ["CoCity.Api/CoCity.Api.csproj", "./CoCity.Api/"]
COPY ["CoCity.Api.Tests/CoCity.Api.Tests.csproj", "./CoCity.Api.Tests/"] 
# 如果有其他项目（如 Shared），请在此处添加类似的 COPY 行

# 2. 还原整个解决方案的依赖
#    使用确切的文件名
RUN dotnet restore "cocity.sln"

# 3. 复制所有项目的源代码到容器中
COPY . .

# 4. 发布主项目 (CoCity.Api 项目)
WORKDIR "/source/CoCity.Api"
RUN dotnet publish "CoCity.Api.csproj" -c Release -o /app/publish --no-restore

# ---- Stage 2: Final ----
# 使用轻量级的 ASP.NET 运行时镜像
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# 容器将监听的端口。ASP.NET Core 8 模板默认监听 8080 (HTTP)
EXPOSE 8080

# 容器启动命令
ENTRYPOINT ["dotnet", "CoCity.Api.dll"]
