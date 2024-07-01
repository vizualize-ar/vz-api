SETLOCAL ENABLEEXTENSIONS
SET storageid=/subscriptions/7e695216-b24c-4241-a46e-d8571e7d2cb2/resourceGroups/tr-dev/providers/Microsoft.Storage/storageAccounts/trdevmedia
SET endpoint=https://8ac14ea3.ngrok.io/image_worker/runtime/webhooks/EventGrid?functionName=GenerateThumbnail

REM ################################################ Product Thumbnails
SET subscription_name=GenerateProductThumbnails
SET container_name=business-product-media

call az eventgrid event-subscription delete ^
	--source-resource-id %storageid% ^
	--name %subscription_name%

call az eventgrid event-subscription create ^
	--source-resource-id %storageid% ^
	--name %subscription_name% ^
	--endpoint %endpoint% ^
	--endpoint-type webhook ^
	--included-event-types Microsoft.Storage.BlobCreated ^
	--subject-begins-with /blobServices/default/containers/%container_name%/

REM ################################################ Business Thumbnails
SET subscription_name=GenerateBusinessThumbnails
SET container_name=business-media

call az eventgrid event-subscription delete ^
	--source-resource-id %storageid% ^
	--name %subscription_name%

call az eventgrid event-subscription create ^
	--source-resource-id %storageid% ^
	--name %subscription_name% ^
	--endpoint %endpoint% ^
	--endpoint-type webhook ^
	--included-event-types Microsoft.Storage.BlobCreated ^
	--subject-begins-with /blobServices/default/containers/%container_name%/

REM ################################################ Review Thumbnails
SET subscription_name=GenerateReviewThumbnails
SET container_name=review-media

call az eventgrid event-subscription delete ^
	--source-resource-id %storageid% ^
	--name %subscription_name%

call az eventgrid event-subscription create ^
	--source-resource-id %storageid% ^
	--name %subscription_name% ^
	--endpoint %endpoint% ^
	--endpoint-type webhook ^
	--included-event-types Microsoft.Storage.BlobCreated ^
	--subject-begins-with /blobServices/default/containers/%container_name%/