using LibrartApp.Domain.Entities;
using LibrartApp.Domain.Enums;
using LibraryApp.Application.Abstractions;
using LibraryApp.Application.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryApp.Test.Application
{
    public class LibraryServiceTest
    {
        private readonly LibraryService _libraryService;
        private readonly Mock<ILibraryAppRepository> _mockRepository;

        public LibraryServiceTest()
        {
        
            _mockRepository = new Mock<ILibraryAppRepository>();
            _libraryService = new LibraryService(_mockRepository.Object);
        }
        [Fact]
        public void WhenABookIsAdded_ThenItShouldBeCreated()
        {
            //Arrange
            var blueDeamonBook = new LibraryItem
            {
                Title = "The Blue Deamon",
                Author = "John Smith",
                Pages = 95,
                Type = (int)LibraryItemTypeEnum.Book,
                IsBorrowed = false
             };
            
            _mockRepository.Setup(r => r.AddLibraryItem(It.IsAny<LibraryItem>()))
                .Callback<LibraryItem>(item => 
                {
                    item.Id = 1; // Simulate database assigning an ID
                });


            //Act
            var result = _libraryService.AddBook(
    blueDeamonBook.Title,
    blueDeamonBook.Author,
    blueDeamonBook.Pages ?? 0);
            //Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }
        //Add more tests here for Get



        //[Fact]
        //public void WhenABookIsAdded_ThenItShouldThrowAnException()
        //{
        //    //Arrange

        //    //Act


        //    //Assert
        //}
        //[Fact]
        //public void WhenAMemberIsRegistered_ThenItShouldBeCreated()
        //{
        //    //Arrange
        //    //Act
        //    //Assert
        //}
    }
}
