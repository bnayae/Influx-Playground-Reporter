###########################################################################################################################
# docker build -t influx-reporter:{tag i.e. version} .														              #
#									#																		              #
# docker push {repo or user-name}/influx-reporter:{tag}														              #
#																											              #
# helm install --name influx-reporter-csharp ./charts/influxreporter --namespace tick						              #
# helm install --name influx-reporter-csharp ./charts/influxreporter --namespace tick --set image.tag={tag}	              #
# helm list																									              #
# helm status influx-reporter-csharp																		              #
#																											              #
# helm upgrade influx-reporter-csharp ./charts/influxreporter --namespace tick -i --recreate-pods  --set image.tag={tag}  #
#																											              #
## Troubleshot ##																							              #
#																											              #
# kubectl logs {pod name} --namespace tick																	              #
#																											              #
###########################################################################################################################

##  Dev layer
FROM microsoft/dotnet:2.2-sdk AS build-env
WORKDIR "/app"

# copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# copy and build everything else
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image (deploy layer)
FROM microsoft/dotnet:2.2-runtime
WORKDIR /app
COPY --from=build-env /app/out .

# log
RUN echo $(pwd)
RUN echo $(ls -1 $pwd)

ENTRYPOINT ["dotnet", "influx-reporter.dll"]