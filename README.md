# API Service

.NET Core based web service running on EC2 instance(windows).  Works with production database and provides various levels of API endpoints.

### Configuration
`/Helpers/Constants.cs`
- `CUSTOMER_PORTAL_URL`: Url of customer portal app
- `STRIPE_KEY`: Stripe private key

### Deploy
Reference guide [here](https://github.com/BidRetriever/Documentation/blob/master/EC2%20Configuration.md)

### CloudFront Configuration
To access API endpoints from HTTPS based front-end apps, the API service needs to be running on HTTPS as well. 
Important thing here is to disable the caching through cache behavior settings.  

- Set `TTL` options to `0`
- Set `Cache Based on Selected Requested Headers` to `All`
