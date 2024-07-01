az eventgrid event-subscription create ^
  --name CreateBusinessReviewRequest-Subscription ^
  --included-event-types NewBusinessOrderCreated ^
  -g tr-qa-eventgrid ^
  --topic-name review-event ^
  --endpoint https://tr-api-qa-worker.azurewebsites.net/CreateBusinessReviewRequest

az eventgrid event-subscription create ^
  --name UpdateReviewInfo-Subscription ^
  --included-event-types BusinessReviewCreated ^
  --endpoint https://tr-api-qa-worker.azurewebsites.net/UpdateReviewInfo ^
  -g tr-qa-eventgrid ^
  --topic-name review-event

az eventgrid event-subscription create ^
  --name UpdateReviewSummaries-Subscription ^
  --included-event-types BusinessReviewCreated ^
  --endpoint https://tr-api-qa-worker.azurewebsites.net/UpdateReviewSummaries ^
  -g tr-qa-eventgrid ^
  --topic-name review-event

az eventgrid event-subscription create ^
  --name UpdateProductReviewImages-Subscription ^
  --included-event-types BusinessReviewCreated ^
  --endpoint https://tr-api-qa-worker.azurewebsites.net/UpdateProductReviewImages ^
  -g tr-qa-eventgrid ^
  --topic-name review-event

az eventgrid event-subscription create ^
  --name UpdateThumbnailImages-Subscription ^
  --included-event-types ImageThumbnailCreated ^
  --endpoint https://tr-api-qa-worker.azurewebsites.net/UpdateThumbnailImages ^
  -g tr-qa-eventgrid ^
  --topic-name review-event

az eventgrid event-subscription create ^
  --name UploadBlockchain-Subscription ^
  --included-event-types  ^
  --endpoint  ^
  -g tr-qa-eventgrid ^
  --topic-name review-event

az eventgrid event-subscription create ^
    --name BlockChainUploadComplete-Subscription ^
    --included-event-types BlockChainUploadComplete ^
    --endpoint https://tr-api-qa-worker.azurewebsites.net/BlockChainUploadComplete ^
    -g tr-qa-eventgrid ^
    --topic-name review-event

az eventgrid event-subscription create ^
    --name ProcessWidgetViewedEvent-Subscription ^
    --included-event-types WidgetViewed ^
    --endpoint https://tr-api-qa-worker.azurewebsites.net/ProcessWidgetViewedEvent ^
    -g tr-qa-eventgrid ^
    --topic-name review-event
