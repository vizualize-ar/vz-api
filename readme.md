## Testing Dev storage account events

1. Run ngrok.bat located in the current directory
2. Create the event in the Azure Storage Account using the ngrok url + the eventgrid path.

For example:
```
https://8fc7898a.ngrok.io/image_worker/runtime/webhooks/EventGrid?functionName=GenerateThumbnail
```
3. Make sure you add a filter for the specific container you're adding the event for.

Examples:
```
Subject begins with: /blobServices/default/containers/review-media/
Subject begins with: /blobServices/default/containers/business-product-media/
```