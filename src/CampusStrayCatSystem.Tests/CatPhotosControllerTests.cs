using CampusStrayCatSystem.Core;
using CampusStrayCatSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace CampusStrayCatSystem.Tests {
    public class CatPhotosControllerTests {
        [Fact] public async Task GetPhotosReturnsNotFoundWhenCatDoesNotExist() {
            var catRepository = new FakeCatRepository { ExistsResult = false };
            var controller = CreateController(catRepository: catRepository);

            var response = await controller.GetPhotos("missing-cat");

            Assert.IsType<NotFoundObjectResult>(response.Result);}

        [Fact] public async Task GetPhotosReturnsEmptyArrayForCatWithoutPhotos() {
            var catRepository = new FakeCatRepository { ExistsResult = true };
            var controller = CreateController(catRepository: catRepository);

            var response = await controller.GetPhotos("test-cat");

            var okResult = Assert.IsType<OkObjectResult>(response.Result);
            var photos = Assert.IsAssignableFrom<IEnumerable<CampusStrayCatSystem.Models.CatPhoto>>(okResult.Value);
            Assert.Empty(photos);}

        [Fact] public async Task GetFeatureReturnsServerErrorForInvalidDatabaseVector() {
            var photoRepository = new FakeCatPhotoRepository {
                FeatureData = new CatPhotoFeatureData {
                    PhotoID = "test-photo",
                    CatID = "test-cat",
                    FeatureVectorJson = "invalid"}};
            var controller = CreateController(photoRepository: photoRepository);

            var response = await controller.GetFeature("test-cat", "test-photo");

            var result = Assert.IsType<ObjectResult>(response.Result);
            Assert.Equal(500, result.StatusCode);}

        [Fact] public async Task DeletePhotoReturnsConflictForReferencedPhoto() {
            var photoRepository = new FakeCatPhotoRepository {
                Photo = new CampusStrayCatSystem.Models.CatPhoto {
                    PhotoID = "test-photo",
                    CatID = "test-cat",
                    PhotoUrl = "/uploads/cats/test-cat/test-photo.png"},
                DeleteStatus = CatPhotoMutationStatus.PhotoReferenced};
            var controller = CreateController(photoRepository: photoRepository);

            var response = await controller.DeletePhoto("test-cat", "test-photo");

            Assert.IsType<ConflictObjectResult>(response);}

        private static CatPhotosController CreateController(FakeCatPhotoRepository? photoRepository = null,
                                                             FakeCatRepository? catRepository = null) {
            var service = new CatPhotoService(photoRepository ?? new FakeCatPhotoRepository(),
                                              new FakeCatPhotoFileStorage(),
                                              catRepository ?? new FakeCatRepository(),
                                              new FakeUserRepository(),
                                              NullLogger<CatPhotoService>.Instance);
            return new CatPhotosController(service);}
    }
}
