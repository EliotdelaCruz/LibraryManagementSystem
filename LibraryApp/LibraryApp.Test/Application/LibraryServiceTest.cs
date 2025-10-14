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
        public void BorrowItem_ShouldSucceed_WhenMemberAndItemAreValid()
        {
            // Arrange
            var memberId = 1;
            var itemId = 10;

            var member = new Member
            {
                Id = memberId,
                Name = "Andrés",
                MembershipStartDate = DateTime.UtcNow.AddDays(-10),
                MembershipEndDate = DateTime.UtcNow.AddDays(10)
            };

            var libraryItem = new LibraryItem
            {
                Id = itemId,
                Title = "Clean Code",
                Author = "Robert C. Martin",
                IsBorrowed = false
            };

            // Simulamos los datos que devolvería el repositorio
            _mockRepository.Setup(r => r.GetMemberById(memberId)).Returns(member);
            _mockRepository.Setup(r => r.GetLibraryItemById(itemId)).Returns(libraryItem);
            _mockRepository.Setup(r => r.GetBorrowedItem(memberId, itemId)).Returns((BorrowItem?)null);

            string message;

            // Act
            var result = _libraryService.BorrowItem(memberId, itemId, out message);

            // Assert
            Assert.True(result);  // ✅ Debe permitir el préstamo
            Assert.Contains("borrowed successfully", message);  // ✅ Mensaje esperado
            _mockRepository.Verify(r => r.AddBorrowedItem(It.Is<BorrowItem>(b =>
                b.MemberId == memberId &&
                b.LibraryItemId == itemId &&
                b.Active == true
            )), Times.Once);
        }
    }

    }
