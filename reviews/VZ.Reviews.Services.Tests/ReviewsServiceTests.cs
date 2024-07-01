using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrueRevue.Reviews.Services.Models;
using TrueRevue.Reviews.Services.Models.Data;
using TrueRevue.Reviews.Services.Models.Response;
using TrueRevue.Reviews.Services.Models.Results;
using TrueRevue.Shared;
using TrueRevue.Shared.EventSchemas.Audio;
using TrueRevue.Shared.EventSchemas.Categories;
using TrueRevue.Shared.EventSchemas.Images;
using TrueRevue.Shared.EventSchemas.Text;
using Moq;
using Xunit;

namespace TrueRevue.Reviews.Services.Tests
{
    public class ReviewsServiceTests
    {
        #region AddCategory Tests
        [Fact]
        public async Task AddCategory_ReturnsDocumentId()
        {
            // arrange
            var service = new ReviewsService(new FakeReviewsRepository(),  new Mock<IEventGridPublisherService>().Object);

            // act
            var result = await service.AddReviewAsync("name", "fakeuserid");

            // assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task AddCategory_AddsDocumentToRepository()
        {
            // arrange
            var fakeCategoriesRepository = new FakeReviewsRepository();
            var service = new ReviewsService(fakeCategoriesRepository,  new Mock<IEventGridPublisherService>().Object);

            // act
            await service.AddReviewAsync("name", "fakeuserid");

            // assert
            Assert.Equal(1, fakeCategoriesRepository.ReviewDocuments.Count);
            Assert.Contains(fakeCategoriesRepository.ReviewDocuments, d => d.name == "name" && d.userId == "fakeuserid");
        }

        [Fact]
        public async Task AddCategory_PublishesCategoryCreatedEventToEventGrid()
        {
            // arrange
            var mockEventGridPublisherService = new Mock<IEventGridPublisherService>();
            var service = new ReviewsService(new FakeReviewsRepository(),  mockEventGridPublisherService.Object);

            // act
            var categoryId = await service.AddReviewAsync("name", "fakeuserid");

            // assert
            mockEventGridPublisherService.Verify(m => 
                    m.PostEventGridEventAsync(EventTypes.Categories.CategoryCreated, 
                        $"fakeuserid/{categoryId}", 
                        It.Is<ReviewCreatedEventData>(d => d.Name == "name")),
                Times.Once);
        }
        #endregion

        #region DeleteCategory Tests
        [Fact]
        public async Task DeleteCategory_ReturnsSuccess()
        {
            // arrange
            var fakeCategoriesRepository = new FakeReviewsRepository();
            fakeCategoriesRepository.ReviewDocuments.Add(new Review { Id = "fakeid", userId = "fakeuserid" });
            var service = new ReviewsService(fakeCategoriesRepository,  new Mock<IEventGridPublisherService>().Object);

            // act
            var result = await service.DeleteReviewAsync("fakeid", "fakeuserid");

            // assert
            Assert.Equal(DeleteReviewResult.Success, result);
        }

        [Fact]
        public async Task DeleteCategory_DeletesDocumentFromRepository()
        {
            // arrange
            var fakeCategoriesRepository = new FakeReviewsRepository();
            fakeCategoriesRepository.ReviewDocuments.Add(new Review { Id = "fakeid", userId = "fakeuserid" });
            var service = new ReviewsService(fakeCategoriesRepository,  new Mock<IEventGridPublisherService>().Object);

            // act
            await service.DeleteReviewAsync("fakeid", "fakeuserid");

            // assert
            Assert.Empty(fakeCategoriesRepository.ReviewDocuments);
        }

        [Fact]
        public async Task DeleteCategory_PublishesCategoryDeletedEventToEventGrid()
        {
            // arrange
            var fakeCategoriesRepository = new FakeReviewsRepository();
            fakeCategoriesRepository.ReviewDocuments.Add(new Review { Id = "fakeid", userId = "fakeuserid" });
            var mockEventGridPublisherService = new Mock<IEventGridPublisherService>();
            var service = new ReviewsService(fakeCategoriesRepository,  mockEventGridPublisherService.Object);

            // act
            await service.DeleteReviewAsync("fakeid", "fakeuserid");

            // assert
            mockEventGridPublisherService.Verify(m => 
                    m.PostEventGridEventAsync(EventTypes.Categories.CategoryDeleted, 
                        "fakeuserid/fakeid", 
                        It.IsAny<ReviewDeletedEventData>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteCategory_InvalidCategoryId_ReturnsNotFound()
        {
            // arrange
            var service = new ReviewsService(new FakeReviewsRepository(),  new Mock<IEventGridPublisherService>().Object);

            // act
            var result = await service.DeleteReviewAsync("fakeid", "fakeuserid");

            // assert
            Assert.Equal(DeleteReviewResult.NotFound, result);
        }

        [Fact]
        public async Task DeleteCategory_IncorrectUserId_ReturnsNotFound()
        {
            // arrange
            var fakeCategoriesRepository = new FakeReviewsRepository();
            fakeCategoriesRepository.ReviewDocuments.Add(new Review { Id = "fakeid", userId = "fakeuserid2" });
            var service = new ReviewsService(fakeCategoriesRepository,  new Mock<IEventGridPublisherService>().Object);

            // act
            var result = await service.DeleteReviewAsync("fakeid", "fakeuserid");

            // assert
            Assert.Equal(DeleteReviewResult.NotFound, result);
        }
        #endregion

        #region UpdateCategory Tests
        [Fact]
        public async Task UpdateCategory_ReturnsSuccess()
        {
            // arrange
            var fakeCategoriesRepository = new FakeReviewsRepository();
            fakeCategoriesRepository.ReviewDocuments.Add(new Review { Id = "fakeid", userId = "fakeuserid" });
            var service = new ReviewsService(fakeCategoriesRepository,  new Mock<IEventGridPublisherService>().Object);

            // act
            var result = await service.UpdateReviewAsync("fakeid", "fakeuserid", "newname");

            // assert
            Assert.Equal(UpdateReviewResult.Success, result);
        }

        [Fact]
        public async Task UpdateCategory_UpdatesDocumentInRepository()
        {
            // arrange
            var fakeCategoriesRepository = new FakeReviewsRepository();
            fakeCategoriesRepository.ReviewDocuments.Add(new Review { Id = "fakeid", Name = "oldname", userId = "fakeuserid" });
            var service = new ReviewsService(fakeCategoriesRepository,  new Mock<IEventGridPublisherService>().Object);
            
            // act
            await service.UpdateReviewAsync("fakeid", "fakeuserid", "newname");

            // assert
            Assert.Equal("newname", fakeCategoriesRepository.ReviewDocuments.Single().name);
        }

        [Fact]
        public async Task UpdateCategory_PublishesCategoryNameUpdatedEventToEventGrid()
        {
            // arrange
            var fakeCategoriesRepository = new FakeReviewsRepository();
            fakeCategoriesRepository.ReviewDocuments.Add(new Review { Id = "fakeid", userId = "fakeuserid"});
            var mockEventGridPublisherService = new Mock<IEventGridPublisherService>();
            var service = new ReviewsService(fakeCategoriesRepository,  mockEventGridPublisherService.Object);

            // act
            await service.UpdateReviewAsync("fakeid", "fakeuserid", "newname");

            // assert
            mockEventGridPublisherService.Verify(m => 
                    m.PostEventGridEventAsync(EventTypes.Categories.CategoryNameUpdated, 
                        "fakeuserid/fakeid",
                        It.Is<ReviewNameUpdatedEventData>(d => d.name == "newname")),
                Times.Once);
        }

        [Fact]
        public async Task UpdateCategory_InvalidCategoryId_ReturnsNotFound()
        {
            // arrange
            var service = new ReviewsService(new FakeReviewsRepository(),  new Mock<IEventGridPublisherService>().Object);

            // act
            var result = await service.UpdateReviewAsync("fakeid", "fakeuserid", "newname");

            // assert
            Assert.Equal(UpdateReviewResult.NotFound, result);
        }

        [Fact]
        public async Task UpdateCategory_IncorrectUserId_ReturnsNotFound()
        {
            // arrange
            var fakeCategoriesRepository = new FakeReviewsRepository();
            fakeCategoriesRepository.ReviewDocuments.Add(new Review { Id = "fakeid", userId = "fakeuserid2" });
            var service = new ReviewsService(fakeCategoriesRepository,  new Mock<IEventGridPublisherService>().Object);

            // act
            var result = await service.UpdateReviewAsync("fakeid", "fakeuserid", "newname");

            // assert
            Assert.Equal(UpdateReviewResult.NotFound, result);
        }
        #endregion

        #region GetCategory Tests
        [Fact]
        public async Task GetCategory_ReturnsCorrectText()
        {
            // arrange
            var fakeCategoriesRepository = new FakeReviewsRepository();
            fakeCategoriesRepository.ReviewDocuments.Add(new Review { Id = "fakeid", name = "fakename", userId = "fakeuserid"});
            var service = new ReviewsService(fakeCategoriesRepository,  new Mock<IEventGridPublisherService>().Object);

            // act
            var result = await service.GetReviewAsync("fakeid", "fakeuserid");

            // assert
            Assert.NotNull(result);
            Assert.Equal("fakeid", result.Id);
            Assert.Equal("fakename", result.name);
        }

        [Fact]
        public async Task GetCategory_InvalidCategoryId_ReturnsNull()
        {
            // arrange
            var service = new ReviewsService(new FakeReviewsRepository(),  new Mock<IEventGridPublisherService>().Object);

            // act
            var result = await service.GetReviewAsync("fakeid", "fakeuserid");

            // assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetCategory_IncorrectUserId_ReturnsNull()
        {
            // arrange
            var fakeCategoriesRepository = new FakeReviewsRepository();
            fakeCategoriesRepository.ReviewDocuments.Add(new Review { Id = "fakeid", name = "fakename", userId = "fakeuserid2"});
            var service = new ReviewsService(fakeCategoriesRepository,  new Mock<IEventGridPublisherService>().Object);

            // act
            var result = await service.GetReviewAsync("fakeid", "fakeuserid");

            // assert
            Assert.Null(result);
        }
        #endregion

        #region ListCategories Tests
        [Fact]
        public async Task ListCategories_ReturnsIds()
        {
            // arrange
            var fakeCategoriesRepository = new FakeReviewsRepository();
            fakeCategoriesRepository.ReviewDocuments.Add(new Review { Id = "fakeid1", name = "fakename1", userId = "fakeuserid" });
            fakeCategoriesRepository.ReviewDocuments.Add(new Review { Id = "fakeid2", name = "fakename2", userId = "fakeuserid" });
            var service = new ReviewsService(fakeCategoriesRepository,  new Mock<IEventGridPublisherService>().Object);

            // act
            var result = await service.GetAllReviewsAsync();

            // assert
            Assert.Equal(2, result.Count);
            var comparer = new CategorySummaryComparer();
            Assert.Contains(new ReviewSummary {Id = "fakeid1", Name = "fakename1"}, result, comparer);
            Assert.Contains(new ReviewSummary { Id = "fakeid2", Name = "fakename2"}, result, comparer);
        }

        [Fact]
        public async Task ListCategories_DoesNotReturnsIdsForAnotherUser()
        {
            // arrange
            var fakeCategoriesRepository = new FakeReviewsRepository();
            fakeCategoriesRepository.ReviewDocuments.Add(new Review { Id = "fakeid1", name = "fakename1", userId = "fakeuserid1" });
            fakeCategoriesRepository.ReviewDocuments.Add(new Review { Id = "fakeid2", name = "fakename2", userId = "fakeuserid2" });
            var service = new ReviewsService(fakeCategoriesRepository,  new Mock<IEventGridPublisherService>().Object);

            // act
            var result = await service.ListReviewAsync("fakeuserid1");

            // assert
            Assert.Single(result);
            var comparer = new CategorySummaryComparer();
            Assert.Contains(new ReviewSummary {Id = "fakeid1", name = "fakename1"}, result, comparer);
        }
        #endregion

        //#region UpdateCategoryImage Tests
        //[Fact]
        //public async Task UpdateCategoryImage_ReturnsTrue()
        //{
        //    // arrange
        //    var fakeReviewRepository = new FakeReviewsRepository();
        //    fakeReviewRepository.CategoryDocuments.Add(new Review { Id = "fakeid", name = "fakename", userId = "fakeuserid" });
        //    var mockImageSearchService = new Mock<IImageSearchService>();
        //    mockImageSearchService
        //        .Setup(m => m.FindImageUrlAsync("fakename"))
        //        .ReturnsAsync("http://fake/imageurl.jpg");
        //    var service = new ReviewsService(fakeReviewRepository, new Mock<IEventGridPublisherService>().Object);

        //    // act
        //    var result = await service.UpdateCategoryImageAsync("fakeid", "fakeuserid");

        //    // assert
        //    Assert.True(result);
        //}

        //[Fact]
        //public async Task UpdateCategoryImage_UpdatesCategoryDocumentWithImageUrl()
        //{
        //    // arrange
        //    var fakeCategoriesRepository = new FakeReviewsRepository();
        //    fakeCategoriesRepository.CategoryDocuments.Add(new Review { Id = "fakeid", name = "fakename", userId = "fakeuserid" });
        //    var mockImageSearchService = new Mock<IImageSearchService>();
        //    mockImageSearchService
        //        .Setup(m => m.FindImageUrlAsync("fakename"))
        //        .ReturnsAsync("http://fake/imageurl.jpg");
        //    var service = new ReviewsService(fakeCategoriesRepository, mockImageSearchService.Object, new Mock<ISynonymService>().Object, new Mock<IEventGridPublisherService>().Object);

        //    // act
        //    await service.UpdateCategoryImageAsync("fakeid", "fakeuserid");

        //    // assert
        //    Assert.Equal("http://fake/imageurl.jpg", fakeCategoriesRepository.CategoryDocuments.Single().ImageUrl);
        //}

        //[Fact]
        //public async Task UpdateCategoryImage_PublishesCategoryImageUpdatedEventToEventGrid()
        //{
        //    // arrange
        //    var fakeCategoriesRepository = new FakeReviewsRepository();
        //    fakeCategoriesRepository.CategoryDocuments.Add(new Review { Id = "fakeid", name = "fakename", userId = "fakeuserid"});
        //    var mockEventGridPublisherService = new Mock<IEventGridPublisherService>();
        //    var mockImageSearchService = new Mock<IImageSearchService>();
        //    mockImageSearchService
        //        .Setup(m => m.FindImageUrlAsync("fakename"))
        //        .ReturnsAsync("http://fake/imageurl.jpg");
        //    var service = new ReviewsService(fakeCategoriesRepository, mockImageSearchService.Object, new Mock<ISynonymService>().Object, mockEventGridPublisherService.Object);

        //    // act
        //    await service.UpdateCategoryImageAsync("fakeid", "fakeuserid");

        //    // assert
        //    mockEventGridPublisherService.Verify(m => 
        //        m.PostEventGridEventAsync(EventTypes.Categories.CategoryImageUpdated, 
        //            "fakeuserid/fakeid", 
        //            It.Is<CategoryImageUpdatedEventData>(c => c.ImageUrl == "http://fake/imageurl.jpg")),
        //        Times.Once);
        //}

        //[Fact]
        //public async Task UpdateCategoryImage_ImageNotFound_ReturnsFalse()
        //{
        //    // arrange
        //    var fakeCategoriesRepository = new FakeReviewsRepository();
        //    fakeCategoriesRepository.CategoryDocuments.Add(new Review { Id = "fakeid", name = "fakename", userId = "fakeuserid" });
        //    var mockImageSearchService = new Mock<IImageSearchService>();
        //    mockImageSearchService
        //        .Setup(m => m.FindImageUrlAsync("fakename"))
        //        .ReturnsAsync((string)null);
        //    var service = new ReviewsService(fakeCategoriesRepository, mockImageSearchService.Object, new Mock<ISynonymService>().Object, new Mock<IEventGridPublisherService>().Object);

        //    // act
        //    var result = await service.UpdateCategoryImageAsync("fakeid", "fakeuserid");

        //    // assert
        //    Assert.False(result);
        //}

        //[Fact]
        //public async Task UpdateCategoryImage_UserIdIncorrect_ReturnsFalse()
        //{
        //    // arrange
        //    var fakeCategoriesRepository = new FakeReviewsRepository();
        //    fakeCategoriesRepository.CategoryDocuments.Add(new Review { Id = "fakeid", name = "fakename", userId = "fakeuserid1" });
        //    var mockImageSearchService = new Mock<IImageSearchService>();
        //    mockImageSearchService
        //        .Setup(m => m.FindImageUrlAsync("fakename"))
        //        .ReturnsAsync("http://fake/imageurl.jpg");
        //    var service = new ReviewsService(fakeCategoriesRepository, mockImageSearchService.Object, new Mock<ISynonymService>().Object, new Mock<IEventGridPublisherService>().Object);

        //    // act
        //    var result = await service.UpdateCategoryImageAsync("fakeid", "fakeuserid2");

        //    // assert
        //    Assert.False(result);
        //}

        //[Fact]
        //public async Task UpdateCategoryImage_UserIdIncorrect_DoesNotUpdateCategoryDocumentWithImageUrl()
        //{
        //    // arrange
        //    var fakeCategoriesRepository = new FakeReviewsRepository();
        //    fakeCategoriesRepository.CategoryDocuments.Add(new Review { Id = "fakeid", name = "fakename", userId = "fakeuserid1" });
        //    var mockImageSearchService = new Mock<IImageSearchService>();
        //    mockImageSearchService
        //        .Setup(m => m.FindImageUrlAsync("fakename"))
        //        .ReturnsAsync("http://fake/imageurl.jpg");
        //    var service = new ReviewsService(fakeCategoriesRepository, mockImageSearchService.Object, new Mock<ISynonymService>().Object, new Mock<IEventGridPublisherService>().Object);

        //    // act
        //    await service.UpdateCategoryImageAsync("fakeid", "fakeuserid2");

        //    // assert
        //    Assert.Null(fakeCategoriesRepository.CategoryDocuments.Single().ImageUrl);
        //}
        //#endregion

        //#region UpdateCategorySynonyms Tests
        //[Fact]
        //public async Task UpdateCategorySynonyms_ReturnsTrue()
        //{
        //    // arrange
        //    var fakeCategoriesRepository = new FakeReviewsRepository();
        //    fakeCategoriesRepository.CategoryDocuments.Add(new Review { Id = "fakeid", name = "fakename", userId = "fakeuserid" });
        //    var mockSynonymService = new Mock<ISynonymService>();
        //    mockSynonymService
        //        .Setup(m => m.GetSynonymsAsync("fakename"))
        //        .ReturnsAsync(new[] { "a", "b" });
        //    var service = new ReviewsService(fakeCategoriesRepository, new Mock<IImageSearchService>().Object, mockSynonymService.Object, new Mock<IEventGridPublisherService>().Object);

        //    // act
        //    var result = await service.UpdateCategorySynonymsAsync("fakeid", "fakeuserid");

        //    // assert
        //    Assert.True(result);
        //}

        //[Fact]
        //public async Task UpdateCategorySynonyms_UpdatesCategoryDocumentWithSynonyms()
        //{
        //    // arrange
        //    var fakeCategoriesRepository = new FakeReviewsRepository();
        //    fakeCategoriesRepository.CategoryDocuments.Add(new Review { Id = "fakeid", name = "fakename", userId = "fakeuserid"});
        //    var mockSynonymService = new Mock<ISynonymService>();
        //    mockSynonymService
        //        .Setup(m => m.GetSynonymsAsync("fakename"))
        //        .ReturnsAsync(new[] { "a", "b" });
        //    var service = new ReviewsService(fakeCategoriesRepository, new Mock<IImageSearchService>().Object, mockSynonymService.Object, new Mock<IEventGridPublisherService>().Object);

        //    // act
        //    await service.UpdateCategorySynonymsAsync("fakeid", "fakeuserid");

        //    // assert
        //    Assert.Equal(2, fakeCategoriesRepository.CategoryDocuments.Single().Synonyms.Count);
        //    Assert.Contains("a", fakeCategoriesRepository.CategoryDocuments.Single().Synonyms);
        //    Assert.Contains("b", fakeCategoriesRepository.CategoryDocuments.Single().Synonyms);
        //}

        //[Fact]
        //public async Task UpdateCategorySynonyms_PublishesCategorySynonymsUpdatedEventToEventGrid()
        //{
        //    // arrange
        //    var fakeCategoriesRepository = new FakeReviewsRepository();
        //    fakeCategoriesRepository.CategoryDocuments.Add(new Review { Id = "fakeid", name = "fakename", userId = "fakeuserid" });
        //    var mockEventGridPublisherService = new Mock<IEventGridPublisherService>();
        //    var mockSynonymService = new Mock<ISynonymService>();
        //    mockSynonymService
        //        .Setup(m => m.GetSynonymsAsync("fakename"))
        //        .ReturnsAsync(new[] { "a", "b" });
        //    var service = new ReviewsService(fakeCategoriesRepository, new Mock<IImageSearchService>().Object, mockSynonymService.Object, mockEventGridPublisherService.Object);

        //    // act
        //    await service.UpdateCategorySynonymsAsync("fakeid", "fakeuserid");

        //    // assert
        //    mockEventGridPublisherService.Verify(m => 
        //        m.PostEventGridEventAsync(EventTypes.Categories.CategorySynonymsUpdated, 
        //            "fakeuserid/fakeid", 
        //            It.Is<CategorySynonymsUpdatedEventData>(c => c.Synonyms.Contains("a") && c.Synonyms.Contains("b"))),
        //        Times.Once);
        //}

        //[Fact]
        //public async Task UpdateCategorySynonyms_SynonymsNotFound_ReturnsFalse()
        //{
        //    // arrange
        //    var fakeCategoriesRepository = new FakeReviewsRepository();
        //    fakeCategoriesRepository.CategoryDocuments.Add(new Review { Id = "fakeid", name = "fakename", userId = "fakeuserid"});
        //    var mockSynonymService = new Mock<ISynonymService>();
        //    mockSynonymService
        //        .Setup(m => m.GetSynonymsAsync("fakename"))
        //        .ReturnsAsync((string[])null);
        //    var service = new ReviewsService(fakeCategoriesRepository, new Mock<IImageSearchService>().Object, mockSynonymService.Object, new Mock<IEventGridPublisherService>().Object);

        //    // act
        //    var result = await service.UpdateCategorySynonymsAsync("fakeid", "fakeuserid");

        //    // assert
        //    Assert.False(result);
        //}

        //[Fact]
        //public async Task UpdateCategorySynonyms_UserIdIncorrect_ReturnsFalse()
        //{
        //    // arrange
        //    var fakeCategoriesRepository = new FakeReviewsRepository();
        //    fakeCategoriesRepository.CategoryDocuments.Add(new Review { Id = "fakeid", name = "fakename", userId = "fakeuserid1"});
        //    var mockSynonymService = new Mock<ISynonymService>();
        //    mockSynonymService
        //        .Setup(m => m.GetSynonymsAsync("fakename"))
        //        .ReturnsAsync(new[] { "a", "b" });
        //    var service = new ReviewsService(fakeCategoriesRepository, new Mock<IImageSearchService>().Object, mockSynonymService.Object, new Mock<IEventGridPublisherService>().Object);

        //    // act
        //    var result = await service.UpdateCategorySynonymsAsync("fakeid", "fakeuserid2");

        //    // assert
        //    Assert.False(result);
        //}

        //[Fact]
        //public async Task UpdateCategorySynonyms_UserIdIncorrect_DoesNotUpdateCategoryDocumentWithSynonyms()
        //{
        //    // arrange
        //    var fakeCategoriesRepository = new FakeReviewsRepository();
        //    fakeCategoriesRepository.CategoryDocuments.Add(new Review { Id = "fakeid", name = "fakename", userId = "fakeuserid1"});
        //    var mockSynonymService = new Mock<ISynonymService>();
        //    mockSynonymService
        //        .Setup(m => m.GetSynonymsAsync("fakename"))
        //        .ReturnsAsync(new[] { "a", "b" });
        //    var service = new ReviewsService(fakeCategoriesRepository, new Mock<IImageSearchService>().Object, mockSynonymService.Object, new Mock<IEventGridPublisherService>().Object);

        //    // act
        //    await service.UpdateCategorySynonymsAsync("fakeid", "fakeuserid2");

        //    // assert
        //    Assert.Empty(fakeCategoriesRepository.CategoryDocuments.Single().Synonyms);
        //}
        //#endregion

        #region ProcessAddItemEvent Tests
        [Fact]
        public async Task ProcessAddItemEventAsync_AddsTextItem()
        {            
            // arrange
            var fakeCategoriesRepository = new FakeReviewsRepository();
            fakeCategoriesRepository.ReviewDocuments.Add(new Review { Id = "fakecategoryid", name = "fakename", userId = "fakeuserid" });
            var service = new ReviewsService(fakeCategoriesRepository,  new Mock<IEventGridPublisherService>().Object);
            var eventToProcess = new EventGridEvent
            {
                Subject = "fakeuserid/fakeitemid", 
                EventType = EventTypes.Text.TextCreated, 
                Data = new TextCreatedEventData
                {
                    Category = "fakecategoryid",
                    Preview = "fakepreview"
                }
            };

            // act
            await service.ProcessAddItemEventAsync(eventToProcess, "fakeuserid");

            // assert
            var itemsCollection = fakeCategoriesRepository.ReviewDocuments.Single().Items;

            Assert.Equal(1, itemsCollection.Count);
            Assert.Contains(new ReviewItem { Id = "fakeitemid", Preview = "fakepreview", Type = ItemType.Text}, itemsCollection, new CategoryItemComparer());
        }

        [Fact]
        public async Task ProcessAddItemEventAsync_AddsImageItem()
        {
            // arrange
            var fakeCategoriesRepository = new FakeReviewsRepository();
            fakeCategoriesRepository.ReviewDocuments.Add(new Review { Id = "fakecategoryid", name = "fakename", userId = "fakeuserid"});
            var service = new ReviewsService(fakeCategoriesRepository,  new Mock<IEventGridPublisherService>().Object);
            var eventToProcess = new EventGridEvent
            {
                Subject = "fakeuserid/fakeitemid", 
                EventType = EventTypes.Images.ImageCreated, 
                Data = new ImageCreatedEventData()
                {
                    Category = "fakecategoryid",
                    PreviewUri = "http://fake/preview.jpg"
                }
            };

            // act
            await service.ProcessAddItemEventAsync(eventToProcess, "fakeuserid");

            // assert
            var itemsCollection = fakeCategoriesRepository.ReviewDocuments.Single().Items;

            Assert.Equal(1, itemsCollection.Count);
            Assert.Contains(new ReviewItem { Id = "fakeitemid", Preview = "http://fake/preview.jpg", Type = ItemType.Image}, itemsCollection, new CategoryItemComparer());
        }

        [Fact]
        public async Task ProcessAddItemEventAsync_AddsAudioItem()
        {
            // arrange
            var fakeCategoriesRepository = new FakeReviewsRepository();
            fakeCategoriesRepository.ReviewDocuments.Add(new Review { Id = "fakecategoryid", name = "fakename", userId = "fakeuserid"});
            var service = new ReviewsService(fakeCategoriesRepository,  new Mock<IEventGridPublisherService>().Object);
            var eventToProcess = new EventGridEvent
            {
                Subject = "fakeuserid/fakeitemid", 
                EventType = EventTypes.Audio.AudioCreated, 
                Data = new AudioCreatedEventData
                {
                    Category = "fakecategoryid",
                    TranscriptPreview = "faketranscript"
                }
            };

            // act
            await service.ProcessAddItemEventAsync(eventToProcess, "fakeuserid");

            // assert
            var itemsCollection = fakeCategoriesRepository.ReviewDocuments.Single().Items;

            Assert.Equal(1, itemsCollection.Count);
            Assert.Contains(new ReviewItem { Id = "fakeitemid", Preview = "faketranscript", Type = ItemType.Audio}, itemsCollection, new CategoryItemComparer());
        }

        [Fact]
        public async Task ProcessAddItemEventAsync_PublishesCategoryItemsUpdatedEventToEventGrid()
        {
            // arrange
            var fakeCategoriesRepository = new FakeReviewsRepository();
            fakeCategoriesRepository.ReviewDocuments.Add(new Review { Id = "fakecategoryid", name = "fakename", userId = "fakeuserid"});
            var mockEventGridPublisherService = new Mock<IEventGridPublisherService>();
            var service = new ReviewsService(fakeCategoriesRepository,  mockEventGridPublisherService.Object);
            var eventToProcess = new EventGridEvent
            {
                Subject = "fakeuserid/fakeitemid", 
                EventType = EventTypes.Audio.AudioCreated, 
                Data = new AudioCreatedEventData
                {
                    Category = "fakecategoryid",
                    TranscriptPreview = "faketranscript"
                }
            };

            // act
            await service.ProcessAddItemEventAsync(eventToProcess, "fakeuserid");

            // assert
            mockEventGridPublisherService.Verify(m => 
                m.PostEventGridEventAsync(EventTypes.Categories.CategoryItemsUpdated,
                    "fakeuserid/fakecategoryid",
                    It.IsAny<ReviewItemsUpdatedEventData>()),
                Times.Once);
        }

        [Fact]
        public async Task ProcessAddItemEventAsync_UpdatesItemWhenAlreadyExists()
        {
            // arrange
            var fakeCategoriesRepository = new FakeReviewsRepository();
            fakeCategoriesRepository.ReviewDocuments.Add(new Review { Id = "fakecategoryid", name = "fakename", userId = "fakeuserid", Items = new List<ReviewItem>() { new ReviewItem { Id = "fakeitemid", Preview = "oldpreview" } } });
            var service = new ReviewsService(fakeCategoriesRepository,  new Mock<IEventGridPublisherService>().Object);
            var eventToProcess = new EventGridEvent
            {
                Subject = "fakeuserid/fakeitemid", 
                EventType = EventTypes.Audio.AudioCreated, 
                Data = new AudioCreatedEventData
                {
                    Category = "fakecategoryid",
                    TranscriptPreview = "newpreview"
                }
            };

            // act
            await service.ProcessAddItemEventAsync(eventToProcess, "fakeuserid");

            // assert
            var itemsCollection = fakeCategoriesRepository.ReviewDocuments.Single().Items;

            Assert.Equal(1, itemsCollection.Count);
            Assert.Contains(new ReviewItem { Id = "fakeitemid", Preview = "newpreview", Type = ItemType.Audio}, itemsCollection, new CategoryItemComparer());
        }

        [Fact]
        public async Task ProcessAddItemEventAsync_DoesNotAddItemWhenUserIdDoesNotMatch()
        {
            // arrange
            var fakeCategoriesRepository = new FakeReviewsRepository();
            fakeCategoriesRepository.ReviewDocuments.Add(new Review { Id = "fakecategoryid", name = "fakename", userId = "fakeuserid1" });
            var service = new ReviewsService(fakeCategoriesRepository,  new Mock<IEventGridPublisherService>().Object);
            var eventToProcess = new EventGridEvent
            {
                Subject = "fakeuserid2/fakeitemid", 
                EventType = EventTypes.Audio.AudioCreated, 
                Data = new AudioCreatedEventData
                {
                    Category = "fakecategoryid",
                    TranscriptPreview = "newpreview"
                }
            };

            // act
            await service.ProcessAddItemEventAsync(eventToProcess, "fakeuserid2");

            // assert
            var itemsCollection = fakeCategoriesRepository.ReviewDocuments.Single().Items;
            Assert.Equal(0, itemsCollection.Count);
        }

        [Fact]
        public async Task ProcessAddItemEventAsync_ThrowsWhenCategoryNotProvided()
        {
            // arrange
            var fakeCategoriesRepository = new FakeReviewsRepository();
            fakeCategoriesRepository.ReviewDocuments.Add(new Review { Id = "fakecategoryid", name = "fakename", userId = "fakeuserid"});
            var service = new ReviewsService(fakeCategoriesRepository,  new Mock<IEventGridPublisherService>().Object);
            var eventToProcess = new EventGridEvent
            {
                Subject = "fakeuserid/fakeitemid", 
                EventType = EventTypes.Audio.AudioCreated, 
                Data = new AudioCreatedEventData
                {
                    TranscriptPreview = "faketranscript"
                }
            };

            // act and assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.ProcessAddItemEventAsync(eventToProcess, "fakeuserid"));
        }
        #endregion
        
        #region ProcessUpdateItemEvent Tests
        [Fact]
        public async Task ProcessUpdateItemEventAsync_UpdatesTextItem()
        {            
            // arrange
            var fakeCategoriesRepository = new FakeReviewsRepository();
            fakeCategoriesRepository.ReviewDocuments.Add(new Review { Id = "fakecategoryid", name = "fakename", userId = "fakeuserid", Items = new List<ReviewItem> { new ReviewItem { Id = "fakeitemid", Type = ItemType.Text, Preview = "oldpreview" } } });
            var service = new ReviewsService(fakeCategoriesRepository,  new Mock<IEventGridPublisherService>().Object);
            var eventToProcess = new EventGridEvent
            {
                Subject = "fakeuserid/fakeitemid", 
                EventType = EventTypes.Text.TextUpdated, 
                Data = new TextUpdatedEventData
                {
                    Preview = "newpreview"
                }
            };

            // act
            await service.ProcessUpdateItemEventAsync(eventToProcess, "fakeuserid");

            // assert
            var itemsCollection = fakeCategoriesRepository.ReviewDocuments.Single().Items;
            Assert.Equal("newpreview", itemsCollection.Single().Preview);
        }

        [Fact]
        public async Task ProcessUpdateItemEventAsync_UpdatesAudioItem()
        {            
            // arrange
            var fakeCategoriesRepository = new FakeReviewsRepository();
            fakeCategoriesRepository.ReviewDocuments.Add(new Review { Id = "fakecategoryid", name = "fakename", userId = "fakeuserid", Items = new List<ReviewItem> { new ReviewItem { Id = "fakeitemid", Type = ItemType.Audio, Preview = "oldpreview" } } });
            var service = new ReviewsService(fakeCategoriesRepository,  new Mock<IEventGridPublisherService>().Object);
            var eventToProcess = new EventGridEvent
            {
                Subject = "fakeuserid/fakeitemid", 
                EventType = EventTypes.Audio.AudioTranscriptUpdated, 
                Data = new AudioTranscriptUpdatedEventData
                {
                    TranscriptPreview = "newpreview"
                }
            };

            // act
            await service.ProcessUpdateItemEventAsync(eventToProcess, "fakeuserid");

            // assert
            var itemsCollection = fakeCategoriesRepository.ReviewDocuments.Single().Items;
            Assert.Equal("newpreview", itemsCollection.Single().Preview);
        }

        [Fact]
        public async Task ProcessUpdateItemEventAsync_PublishesCategoryItemsUpdatedEventToEventGrid()
        {            
            // arrange
            var fakeCategoriesRepository = new FakeReviewsRepository();
            fakeCategoriesRepository.ReviewDocuments.Add(new Review { Id = "fakecategoryid", name = "fakename", userId = "fakeuserid", Items = new List<ReviewItem> { new ReviewItem { Id = "fakeitemid", Type = ItemType.Text, Preview = "oldpreview" } } });
            var mockEventGridPublisherService = new Mock<IEventGridPublisherService>();
            var service = new ReviewsService(fakeCategoriesRepository,  mockEventGridPublisherService.Object);
            var eventToProcess = new EventGridEvent
            {
                Subject = "fakeuserid/fakeitemid", 
                EventType = EventTypes.Text.TextUpdated, 
                Data = new TextUpdatedEventData
                {
                    Preview = "newpreview"
                }
            };

            // act
            await service.ProcessUpdateItemEventAsync(eventToProcess, "fakeuserid");

            // assert
            mockEventGridPublisherService.Verify(m => 
                m.PostEventGridEventAsync(EventTypes.Categories.CategoryItemsUpdated,
                    "fakeuserid/fakecategoryid",
                    It.IsAny<ReviewItemsUpdatedEventData>()),
                Times.Once);
        }
        #endregion
        
        #region ProcessDeleteItemEvent Tests
        [Fact]
        public async Task ProcessDeleteItemEventAsync_DeletesTextItem()
        {            
            // arrange
            var fakeCategoriesRepository = new FakeReviewsRepository();
            fakeCategoriesRepository.ReviewDocuments.Add(new Review { Id = "fakecategoryid", name = "fakename", userId = "fakeuserid", Items = new List<ReviewItem> { new ReviewItem { Id = "fakeitemid", Type = ItemType.Text } } });
            var service = new ReviewsService(fakeCategoriesRepository,  new Mock<IEventGridPublisherService>().Object);
            var eventToProcess = new EventGridEvent
            {
                Subject = "fakeuserid/fakeitemid", 
                EventType = EventTypes.Text.TextDeleted, 
                Data = new TextDeletedEventData()
            };

            // act
            await service.ProcessDeleteItemEventAsync(eventToProcess, "fakeuserid");

            // assert
            Assert.Empty(fakeCategoriesRepository.ReviewDocuments.Single().Items);
        }

        [Fact]
        public async Task ProcessDeleteItemEventAsync_DeletesAudioItem()
        {            
            // arrange
            var fakeCategoriesRepository = new FakeReviewsRepository();
            fakeCategoriesRepository.ReviewDocuments.Add(new Review { Id = "fakecategoryid", name = "fakename", userId = "fakeuserid", Items = new List<ReviewItem> { new ReviewItem { Id = "fakeitemid", Type = ItemType.Audio } } });
            var service = new ReviewsService(fakeCategoriesRepository,  new Mock<IEventGridPublisherService>().Object);
            var eventToProcess = new EventGridEvent
            {
                Subject = "fakeuserid/fakeitemid", 
                EventType = EventTypes.Audio.AudioDeleted, 
                Data = new AudioDeletedEventData()
            };

            // act
            await service.ProcessDeleteItemEventAsync(eventToProcess, "fakeuserid");

            // assert
            Assert.Empty(fakeCategoriesRepository.ReviewDocuments.Single().Items);
        }

        [Fact]
        public async Task ProcessDeleteItemEventAsync_PublishesCategoryItemsUpdatedEventToEventGrid()
        {            
            // arrange
            var fakeCategoriesRepository = new FakeReviewsRepository();
            fakeCategoriesRepository.ReviewDocuments.Add(new Review { Id = "fakecategoryid", name = "fakename", userId = "fakeuserid", Items = new List<ReviewItem> { new ReviewItem { Id = "fakeitemid", Type = ItemType.Audio } } });
            var mockEventGridPublisherService = new Mock<IEventGridPublisherService>();
            var service = new ReviewsService(fakeCategoriesRepository,  mockEventGridPublisherService.Object);
            var eventToProcess = new EventGridEvent
            {
                Subject = "fakeuserid/fakeitemid", 
                EventType = EventTypes.Audio.AudioDeleted, 
                Data = new AudioDeletedEventData()
            };

            // act
            await service.ProcessDeleteItemEventAsync(eventToProcess, "fakeuserid");

            // assert
            mockEventGridPublisherService.Verify(m => 
                m.PostEventGridEventAsync(EventTypes.Categories.CategoryItemsUpdated,
                    "fakeuserid/fakecategoryid",
                    It.IsAny<ReviewItemsUpdatedEventData>()),
                Times.Once);
        }
        #endregion

        #region Helpers
        private class CategorySummaryComparer: IEqualityComparer<ReviewSummary>
        {
            public bool Equals(ReviewSummary x, ReviewSummary y) => x.Id == y.Id &&
                                                                        x.Name == y.Name;

            public int GetHashCode(ReviewSummary obj) => obj.GetHashCode();
        }

        private class CategoryItemComparer: IEqualityComparer<ReviewItem>
        {
            public bool Equals(ReviewItem x, ReviewItem y) => x.Id == y.Id &&
                                                                  x.Preview == y.Preview && 
                                                                  x.Type == y.Type;

            public int GetHashCode(ReviewItem obj) => obj.GetHashCode();
        }
        #endregion
    }
}
