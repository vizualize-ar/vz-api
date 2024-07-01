az eventgrid event-subscription create ^
  --name CreateBusinessReviewRequest-Subscription ^
  --included-event-types NewBusinessOrderCreated ^
  -g tr-prod-eventgrid ^
  --topic-name tr-prod-events ^
  --endpoint https://tr-prod-api-worker.azurewebsites.net/CreateBusinessReviewRequest

az eventgrid event-subscription create ^
  --name UpdateReviewInfo-Subscription ^
  --included-event-types BusinessReviewCreated ^
  --endpoint https://tr-prod-api-worker.azurewebsites.net/UpdateReviewInfo ^
  -g tr-prod-eventgrid ^
  --topic-name tr-prod-events

az eventgrid event-subscription create ^
  --name UpdateReviewSummaries-Subscription ^
  --included-event-types BusinessReviewCreated ^
  --endpoint https://tr-prod-api-worker.azurewebsites.net/UpdateReviewSummaries ^
  -g tr-prod-eventgrid ^
  --topic-name tr-prod-events

az eventgrid event-subscription create ^
  --name UpdateProductReviewImages-Subscription ^
  --included-event-types BusinessReviewCreated ^
  --endpoint https://tr-prod-api-worker.azurewebsites.net/UpdateProductReviewImages ^
  -g tr-prod-eventgrid ^
  --topic-name tr-prod-events

az eventgrid event-subscription create ^
  --name UpdateThumbnailImages-Subscription ^
  --included-event-types ImageThumbnailCreated ^
  --endpoint https://tr-prod-api-worker.azurewebsites.net/UpdateThumbnailImages ^
  -g tr-prod-eventgrid ^
  --topic-name tr-prod-events

az eventgrid event-subscription create ^
  --name UploadBlockchain-Subscription ^
  --included-event-types UploadBlockchain ^
  --endpoint  https://tr-prod-api-blockchain.azurewebsites.net/runtime/webhooks/EventGrid?functionName=BlockChainUpload&code=FJTmdqrQRePWsUUfBeZue2CbcH2Q5hHHaFr6so2pRjavyBEwW6u2OA== ^
  -g tr-prod-eventgrid ^
  --topic-name tr-prod-events

az eventgrid event-subscription create ^
  --name BlockChainUploadComplete-Subscription ^
  --included-event-types BlockChainUploadComplete ^
  --endpoint https://tr-prod-api-worker.azurewebsites.net/BlockChainUploadComplete ^
  -g tr-prod-eventgrid ^
  --topic-name tr-prod-events

az eventgrid event-subscription create ^
  --name ProcessWidgetViewedEvent-Subscription ^
  --included-event-types WidgetViewed ^
  --endpoint https://tr-prod-api-worker.azurewebsites.net/ProcessWidgetViewedEvent ^
  -g tr-prod-eventgrid ^
  --topic-name tr-prod-events
