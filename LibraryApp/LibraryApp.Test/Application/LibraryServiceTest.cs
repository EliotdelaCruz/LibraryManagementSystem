using LibrartApp.Domain.Entities;
using LibrartApp.Domain.Enums;
using LibraryApp.Application.Abstractions;
using LibraryApp.Application.Services;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
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

        //Create a Unit Test to Register a Member
        [Fact]
        public void WhenAMemberIsRegistered_ThenItShouldBeCreated()
        {
            //Arrange
            var memberName = "Alice Johnson";
            var memberEntity = new Member
            {
                Name = memberName,
                MembershipStartDate = DateTime.UtcNow,
                MembershipEndDate = DateTime.UtcNow.AddYears(1)
            };
            _mockRepository.Setup(r => r.AddMember(It.IsAny<Member>()))
                .Callback<Member>(member =>
                {
                    member.Id = 1; // Simulate database assigning an ID
                });
            //Act
            var result = _libraryService.RegisterMember(memberName);
            //Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(memberName, result.Name);
        }

        //Create a Unit Test to Borrow an Item
        [Fact]
        public void WhenAnItemIsBorrowed_ThenItShouldBeMarkedAsBorrowed()
        {
            //Arrange
            var memberId = 1;
            var itemId = 1;
            string message = string.Empty;
            var memberEntity = new Member
            {
                Id = memberId,
                Name = "Alice Johnson",
                MembershipStartDate = DateTime.UtcNow,
                MembershipEndDate = DateTime.UtcNow.AddYears(1)
            };
            var bookEntity = new LibraryItem
            {
                Id = itemId,
                Title = "The Blue Deamon",
                Author = "John Smith",
                Pages = 95,
                Type = (int)LibraryItemTypeEnum.Book,
                IsBorrowed = false
            };
            _mockRepository.Setup(r => r.GetMemberById(memberId)).Returns(memberEntity);
            _mockRepository.Setup(r => r.GetLibraryItemById(itemId)).Returns(bookEntity);
            _mockRepository.Setup(r => r.UpdateLibraryItem(It.IsAny<LibraryItem>()))
                .Callback<LibraryItem>(item =>
                {
                    // Simulate updating the item in the database
                });
            //Act
            _libraryService.BorrowItem(memberId, itemId,out message);
            //Assert
            Assert.True(bookEntity.IsBorrowed);
        }

        }
}
