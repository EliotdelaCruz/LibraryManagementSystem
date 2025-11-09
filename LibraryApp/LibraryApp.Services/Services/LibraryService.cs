using LibrartApp.Domain;
using LibrartApp.Domain.Entities;
using LibrartApp.Domain.Enums;
using LibraryApp.Application.Abstractions;


namespace LibraryApp.Application.Services
{
    public sealed class LibraryService : ILibraryService
    {
        private readonly ILibraryAppRepository _repository;
        //lista privada como almacenamiento temporal
        private readonly List<BorrowItem> _borrowedItems = new();
        private readonly List<LibrartApp.Domain.Entities.Member> _members = new();

        public LibraryService(ILibraryAppRepository repository)
        {
            _repository = repository;
        }
        public Book AddBook(string title, string author, int pages = 0)
        {
            var bookEntity = new LibrartApp.Domain.Entities.LibraryItem
            {
                Title = title,
                Author = author,
                Pages = pages,
                Type = (int)LibraryItemTypeEnum.Book,
                IsBorrowed = false
            };
            //if(bookEntity.Pages > 100) 
            //    throw new ArgumentOutOfRangeException(nameof(pages), "Books with more than 100 pages are not allowed.");
            _repository.AddLibraryItem(bookEntity);

            return new Book(bookEntity.Id, bookEntity.Title, bookEntity.Author);
        }
        public Magazine AddMagazine(string title, int issueNumber, string publisher)
        {
            var magEntity = new LibrartApp.Domain.Entities.LibraryItem
            {
                Title = title,
                IssueNumber = (int)issueNumber,
                Publisher = publisher,
                Type = (int)LibraryItemTypeEnum.Magazine,
                IsBorrowed = false
            };

            _repository.AddLibraryItem(magEntity);

            return new LibrartApp.Domain.Magazine(magEntity.Id, magEntity.Title, magEntity.IssueNumber ?? 0, magEntity.Publisher);
        }
        public LibrartApp.Domain.Member RegisterMember(string name)
        {
            var memberEntity = new LibrartApp.Domain.Entities.Member
            {
                Name = name,
                MembershipStartDate = DateTime.UtcNow,
                MembershipEndDate = DateTime.UtcNow.AddYears(1)
            };

            _repository.AddMember(memberEntity);

            return new LibrartApp.Domain.Member(memberEntity.Id, memberEntity.Name);
        }
        public IEnumerable<LibrartApp.Domain.LibraryItem> FindItems(string? term)
        {
            //if (string.IsNullOrWhiteSpace(term)) return _items;
            //term = term.Trim().ToLowerInvariant();
            //return _items.Where(i => i.Title.ToLowerInvariant().Contains(term));
            throw new NotImplementedException();
        }
        public bool BorrowItem(int memberId, int itemId, out string message)
        {
            message = "";

            var member = _members.FirstOrDefault(m => m.Id == memberId);
            if (member == null)
            {
                message = "Member not found.";
                return false;
            }

            // 1️⃣ Validar membresía vigente
            if (DateTime.UtcNow > member.MembershipEndDate)
            {
                message = "Membership expired — please renew before borrowing items.";
                return false;
            }

            // 2️⃣ Validar límite de 3 préstamos activos
            var activeBorrowCount = _borrowedItems.Count(b => b.MemberId == memberId && b.Active);
            if (activeBorrowCount >= 3)
            {
                message = "Borrow limit reached — members can only borrow up to 3 items.";
                return false;
            }

            // 3️⃣ Validar préstamos vencidos
            var hasExpiredBorrow = _borrowedItems.Any(b =>
                b.MemberId == memberId && b.DueDate < DateTime.UtcNow && b.Active);

            if (hasExpiredBorrow)
            {
                message = "You have overdue items — please return them before borrowing new ones.";
                return false;
            }

            // 4️⃣ Registrar préstamo nuevo
            var borrow = new BorrowItem
            {
                MemberId = memberId,
                LibraryItemId = itemId,
                BorrowDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(3),
                Active = true
            };

            _borrowedItems.Add(borrow);
            message = $"Item {itemId} borrowed successfully. Due date: {borrow.DueDate:MM/dd/yyyy}.";
            return true;
        }




        public bool ReturnItem(int memberId, int itemId, out string message)
        {
            message = "";

            var borrow = _borrowedItems.FirstOrDefault(b => b.MemberId == memberId && b.LibraryItemId == itemId && b.Active);
            if (borrow == null)
            {
                message = "No active borrow found for this item.";
                return false;
            }

            borrow.Active = false;
            borrow.DueDate = DateTime.UtcNow;

            message = $"Item {itemId} returned successfully.";
            return true;
        }

        public IEnumerable<BorrowItem> GetBorrowedItemsByMember(int memberId)
        {
            return _borrowedItems
                .Where(b => b.MemberId == memberId && b.Active)
                .ToList();
        }
        public IEnumerable<LibrartApp.Domain.LibraryItem> GetAllLibraryItems()
        {
            var libraryItemsEntities = _repository.GetAllLibraryItems();
            return libraryItemsEntities.Select(MapToDomainModel);
        }

        private LibrartApp.Domain.LibraryItem MapToDomainModel(LibrartApp.Domain.Entities.LibraryItem entity)
        {
            if (entity.IsBorrowed)
            {
                switch ((LibraryItemTypeEnum)entity.Type)
                {
                    case LibraryItemTypeEnum.Book:
                        var book = new Book(entity.Id, entity.Title, entity.Author ?? string.Empty, entity.Pages ?? 0);
                        book.Borrow();
                        return book;
                    case LibraryItemTypeEnum.Magazine:
                        var mag = new Magazine(entity.Id, entity.Title, entity.IssueNumber ?? 0, entity.Publisher ?? string.Empty);
                        mag.Borrow();
                        return mag;
                    default:
                        throw new InvalidOperationException("Unknown library item type.");

                }
            }

            return (LibraryItemTypeEnum)entity.Type switch
            {
                LibraryItemTypeEnum.Book => new Book(entity.Id, entity.Title, entity.Author ?? string.Empty, entity.Pages ?? 0),
                LibraryItemTypeEnum.Magazine => new Magazine(entity.Id, entity.Title, entity.IssueNumber ?? 0, entity.Publisher ?? string.Empty),
                _ => throw new InvalidOperationException("Unknown library item type.")
            };
        }

        public IEnumerable<LibrartApp.Domain.Member> GetAllMembers()
        {
            var membersEntities = _repository.GetAllMembers();
            return membersEntities.Select(MapToDomainMembersModel);
        }

        private LibrartApp.Domain.Member MapToDomainMembersModel(LibrartApp.Domain.Entities.Member entity)
        {
            return new LibrartApp.Domain.Member(entity.Id, entity.Name);

            // Unfinished attemp for returning BorrowedItems
            var member = new LibrartApp.Domain.Member(entity.Id, entity.Name);
            foreach (var itemId in entity.BorrowedItems)
            {

            }
        }
    }
}