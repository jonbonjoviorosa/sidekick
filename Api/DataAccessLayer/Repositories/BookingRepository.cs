using Microsoft.EntityFrameworkCore;
using Sidekick.Api.DataAccessLayer.Interfaces;
using Sidekick.Api.Helpers;
using Sidekick.Api.ViewModel;
using Sidekick.Model;
using Sidekick.Model.Booking;
using Sidekick.Model.Class;
using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity.SqlServer;


namespace Sidekick.Api.DataAccessLayer.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly APIDBContext context;

        public BookingRepository(APIDBContext context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<IndividualBookingViewModel>> GetIndividualBookingsPerParticipant(Guid userId, bool getLatest)
        {
            var listindividual = new List<IndividualBookingViewModel>();
            var query = from x in context.IndividualBookings
                        join y in context.IndividualClasses
                            on x.ClassId equals y.ClassId
                        join z in context.Users
                            on y.CoachId equals z.UserId
                        where x.TraineeId == userId
                        select new IndividualBookingViewModel
                        {
                            BookingId = x.BookingId,
                            ClassId = y.ClassId,
                            CoachId = y.CoachId,
                            CoachFirstName = z.FirstName,
                            CoachLastName = z.LastName,
                            TraineeId = x.TraineeId,
                            Date = x.Date,
                            StartTime = x.StartTime,
                            EndTime = x.EndTime,
                            Coaching = x.Coaching,
                            Location = x.Location,
                            Notes = x.Notes,
                            TransactionNo = x.TransactionNo,
                            Status = x.Status,
                            UserImage = z.ImageUrl,
                            BookingAmount = (double)x.TotalAmount,
                            BookingType = EBookingType.Individual
                        };

            var queryGroup = from x in context.GroupBookings
                             join y in context.GroupClasses
                                 on x.GroupClassId equals y.GroupClassId
                             join z in context.Users
                                 on y.CoachId equals z.UserId
                             where x.ParticipantId == userId
                             select new IndividualBookingViewModel
                             {
                                 BookingId = x.GroupBookingId,
                                 CoachId = y.CoachId,
                                 ClassId = y.GroupClassId,
                                 CoachFirstName = z.FirstName,
                                 CoachLastName = z.LastName,
                                 TraineeId = x.ParticipantId,
                                 Date = x.Date.GetValueOrDefault(),
                                 StartTime = y.StartTime,
                                 GroupDuration = y.Duration.GetValueOrDefault(),
                                 Coaching = y.Title,
                                 UserImage = z.ImageUrl,
                                 TransactionNo = x.TransactionNo,
                                 Location = context.Gyms.Where(x => x.GymId == x.GymId).Select(x => x.GymName).FirstOrDefault(),
                                 Notes = 0,
                                 Status = x.Status,
                                 BookingAmount = (double)x.TotalAmount,
                                 GroupNotes = y.Notes,
                                 BookingType = EBookingType.Group
                             };




            listindividual = await query.ToListAsync();
            var groupbooking = await queryGroup.ToListAsync();
            listindividual.AddRange(groupbooking);
            return listindividual;
        }

        public async Task<IEnumerable<IndividualBookingViewModel>> GetAllBookingsPerParticipant(Guid userId)
        {
            var listindividual = new List<IndividualBookingViewModel>();
            var query = from x in context.IndividualBookings
                        join y in context.IndividualClasses
                            on x.ClassId equals y.ClassId
                        join z in context.Users
                            on y.CoachId equals z.UserId
                        where x.TraineeId == userId
                        select new IndividualBookingViewModel
                        {
                            BookingId = x.BookingId,
                            ClassId = y.ClassId,
                            CoachId = y.CoachId,
                            CoachFirstName = z.FirstName,
                            CoachLastName = z.LastName,
                            TraineeId = x.TraineeId,
                            Date = x.Date,
                            StartTime = x.StartTime,
                            EndTime = x.EndTime,
                            Coaching = x.Coaching,
                            Location = x.Location,
                            Notes = x.Notes,
                            TransactionNo = x.TransactionNo,
                            Status = x.Status,
                            UserImage = z.ImageUrl,
                            BookingAmount = (double)x.TotalAmount,
                            VatAmount = (double)x.ServiceFees,
                            BookingType = EBookingType.Individual
                        };

            var queryGroup = from x in context.GroupBookings
                             join y in context.GroupClasses
                                 on x.GroupClassId equals y.GroupClassId
                             join z in context.Users
                                 on y.CoachId equals z.UserId
                             where x.ParticipantId == userId
                             select new IndividualBookingViewModel
                             {
                                 BookingId = x.GroupBookingId,
                                 CoachId = y.CoachId,
                                 ClassId = y.GroupClassId,
                                 CoachFirstName = z.FirstName,
                                 CoachLastName = z.LastName,
                                 TraineeId = x.ParticipantId,
                                 Date = x.Date.GetValueOrDefault(),
                                 StartTime = y.StartTime,
                                 GroupDuration = y.Duration.GetValueOrDefault(),
                                 Coaching = y.Title,
                                 UserImage = z.ImageUrl,
                                 TransactionNo = x.TransactionNo,
                                 Location = context.Gyms.Where(x => x.GymId == x.GymId).Select(x => x.GymName).FirstOrDefault(),
                                 Notes = 0,
                                 Status = x.Status,
                                 BookingAmount = (double)x.TotalAmount,
                                 VatAmount = (double)x.ServiceFees,
                                 CommissionAmount = (double)x.SideKickCommission,
                                 GroupNotes = y.Notes,
                                 BookingType = EBookingType.Group
                             };

            var queryPlay = from x in context.FacilityPlayers
                             join z in context.Users
                                 on x.UserId equals z.UserId
                             join up in context.UserPitchBookings
                                 on x.BookingId equals up.BookingId
                             where x.UserId == userId
                             select new IndividualBookingViewModel
                             {
                                 BookingId = x.BookingId,
                                 CoachId = Guid.Empty,
                                 ClassId = Guid.Empty,
                                 CoachFirstName = z.FirstName,
                                 CoachLastName = z.LastName,
                                 TraineeId = x.UserId,
                                 Date = up.Date,
                                 StartTime = "",
                                 GroupDuration =0,
                                 Coaching = up.Name,
                                 UserImage = z.ImageUrl,
                                 TransactionNo = x.InitialTransactionNo,
                                 Location =string.Empty,
                                 Notes = 0,
                                 Status = (EBookingStatus)x.PlayerStatus,
                                 BookingAmount = (double)x.TotalAmount,
                                 CommissionAmount = 0, //Todo
                                 GroupNotes = null,
                                 VatAmount = 0,
                                 BookingType = EBookingType.Play
                             };


            listindividual = await query.ToListAsync();
            var groupbooking = await queryGroup.ToListAsync();
            listindividual.AddRange(groupbooking);
            return listindividual;
        }

        public async Task<IEnumerable<IndividualBookingViewModel>> GetAllBookings()
        {
            var listindividual = new List<IndividualBookingViewModel>();
            var query = from x in context.IndividualBookings
                        join y in context.IndividualClasses
                            on x.ClassId equals y.ClassId
                        join z in context.Users
                            on x.TraineeId equals z.UserId
                        //where x.Status == EBookingStatus.Confirmed
                        select new IndividualBookingViewModel
                        {
                            BookingId = x.BookingId,
                            ClassId = y.ClassId,
                            CoachId = y.CoachId,
                            CoachFirstName = z.FirstName,
                            CoachLastName = z.LastName,
                            TraineeId = x.TraineeId,
                            Date = x.Date,
                            StartTime = x.StartTime,
                            EndTime = x.EndTime,
                            Coaching = x.Coaching,
                            Location = x.Location,
                            Notes = x.Notes,
                            TransactionNo = x.TransactionNo,
                            Status = x.Status,
                            UserImage = z.ImageUrl,
                            BookingAmount = (double)x.TotalAmount,
                            VatAmount = (double)x.ServiceFees,
                            BookingType = EBookingType.Individual
                        };

            var queryGroup = from x in context.GroupBookings
                             join y in context.GroupClasses
                                 on x.GroupClassId equals y.GroupClassId
                             join z in context.Users
                                 on x.ParticipantId equals z.UserId
                             //where x.Status == EBookingStatus.Confirmed
                             select new IndividualBookingViewModel
                             {
                                 BookingId = x.GroupBookingId,
                                 CoachId = y.CoachId,
                                 ClassId = y.GroupClassId,
                                 CoachFirstName = z.FirstName,
                                 CoachLastName = z.LastName,
                                 TraineeId = x.ParticipantId,
                                 Date = x.Date.GetValueOrDefault(),
                                 StartTime = y.StartTime,
                                 EndDate = x.Date.GetValueOrDefault().AddHours(y.Duration.GetValueOrDefault()),
                                 EndTime = !string.IsNullOrWhiteSpace(y.StartTime) ? TimeSpan.Parse(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, TimeSpan.Parse(y.StartTime).Hours, TimeSpan.Parse(y.StartTime).Minutes, 0).AddHours((int)y.Duration.GetValueOrDefault()).ToString("HH:mm")).ToString().Substring(0, 5) : "",
                                 GroupDuration = y.Duration.GetValueOrDefault(),
                                 Coaching = y.Title,
                                 UserImage = z.ImageUrl,
                                 TransactionNo = x.TransactionNo,
                                 Location = context.Gyms.Where(x => x.GymId == x.GymId).Select(x => x.GymName).FirstOrDefault(),
                                 Notes = 0,
                                 Status = x.Status,
                                 BookingAmount = (double)x.TotalAmount,
                                 VatAmount = (double)x.ServiceFees,
                                 CommissionAmount = (double)x.SideKickCommission,
                                 GroupNotes = y.Notes,
                                 BookingType = EBookingType.Group
                             };

            listindividual = await query.ToListAsync();
            var groupbooking = await queryGroup.ToListAsync();
            listindividual.AddRange(groupbooking);
            return listindividual;
        }

        public async Task<IEnumerable<IndividualBookingViewModel>> GetIndividualBookingsPerCoach(Guid userId, bool getLatest)
        {
            var query = from x in context.IndividualBookings
                        join y in context.IndividualClasses
                            on x.ClassId equals y.CoachId
                        join z in context.Users
                            on y.CoachId equals z.UserId
                        where y.CoachId == userId
                        select new IndividualBookingViewModel
                        {
                            BookingId = x.BookingId,
                            CoachId = y.CoachId,
                            CoachFirstName = z.FirstName,
                            CoachLastName = z.LastName,
                            TraineeId = x.TraineeId,
                            Date = x.Date,
                            StartTime = x.StartTime,
                            EndTime = x.EndTime,
                            Coaching = x.Coaching,
                            Location = x.Location,
                            Notes = x.Notes,
                            Status = x.Status
                        };
            if (getLatest)
            {
                return await query.Where(x => x.Date.Date >= Helper.GetDateTime().Date)
                    .ToListAsync();
            }
            else
            {
                return await query.Where(x => x.Date.Date < Helper.GetDateTime().Date)
                    .ToListAsync();
            }
        }

        public async Task<IndividualBooking> GetIndividualBooking(Guid bookingId)
        {
            return await context.IndividualBookings
                .Where(x => x.BookingId == bookingId)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<CancelPlayBookingViewModel>> GetBookingReferences(Guid facilityPitchTimingId)
        {
            var cancelPlayBookingVm = new List<CancelPlayBookingViewModel>();
            var facilityPlayers = await context.FacilityPlayers.ToListAsync();
            var userPitchBookings = await context.UserPitchBookings.Where(f => f.FacilityPitchTimingId == facilityPitchTimingId).ToListAsync();
            var timing = await context.FacilityPitchTimings.Where(f => f.FacilityPitchTimingId == facilityPitchTimingId).FirstOrDefaultAsync();
            var facilities = await context.Facilities.Include(f => f.Area).ToListAsync();
            var sports = await context.Sports.ToListAsync();
            if (userPitchBookings.Any())
            {
                foreach (var item in userPitchBookings)
                {
                    var facility = facilities.Where(f => f.FacilityId == item.FacilityId).FirstOrDefault();
                    var facilityPlayer = facilityPlayers.Where(f => f.UserId == item.UserId && f.BookingId == item.BookingId).FirstOrDefault();
                    if (facilityPlayer != null)
                    {
                        cancelPlayBookingVm.Add(new CancelPlayBookingViewModel
                        {
                            FacilityPitchTimingId = item.FacilityPitchTimingId,
                            FacilityPitchId = item.FacilityPitchId,
                            TelRRefNo = facilityPlayer.TelRRefNo,
                            TransactionNo = facilityPlayer.InitialTransactionNo,
                            TotalAmount = facilityPlayer.TotalAmount,
                            IsFree = item.IsFree,
                            BookingId = item.BookingId,
                            Day = timing.Day,
                            Start = timing.TimeStart,
                            End = timing.TimeEnd,
                            Player = facilityPlayer,
                            BookingDate = item.Date,
                            Location = facility.Area.AreaName != null ? facility.Area.AreaName : String.Empty,
                            Facility = facility.Name,
                            PricePerPlayer = item.PricePerUser != null ? item.PricePerUser.Value : default,
                            Sport = sports.Where(s => s.SportId == item.SportId).FirstOrDefault() != null ? sports.Where(s => s.SportId == item.SportId).FirstOrDefault().Name : String.Empty
                        });
                    }
                }

            }

            return cancelPlayBookingVm;
        }


        public async Task<IEnumerable<CancelPlayBookingViewModel>> GetBookingReferencesByBookingId(Guid bookingId)
        {
            var cancelPlayBookingVm = new List<CancelPlayBookingViewModel>();
            var facilityPlayers = await context.FacilityPlayers.AsNoTracking().Where(s=>s.BookingId==bookingId).ToListAsync();
            var userPitchBookings = await context.UserPitchBookings.AsNoTracking().Where(f => f.BookingId == bookingId).ToListAsync();
            var facilities = await context.Facilities.AsNoTracking().Include(f => f.Area).ToListAsync();
            var sports = await context.Sports.AsNoTracking().ToListAsync();
            if (userPitchBookings.Any())
            {
                foreach (var item in userPitchBookings)
                {
                    var facility = facilities.Where(f => f.FacilityId == item.FacilityId).FirstOrDefault();
                    var facilityPlayer = facilityPlayers.Where(f => f.BookingId == item.BookingId).FirstOrDefault();
                    if (facilityPlayer != null)
                    {
                        cancelPlayBookingVm.Add(new CancelPlayBookingViewModel
                        {
                            FacilityPitchTimingId = item.FacilityPitchTimingId,
                            FacilityPitchId = item.FacilityPitchId,
                            TelRRefNo = facilityPlayer.TelRRefNo,
                            TransactionNo = facilityPlayer.InitialTransactionNo,
                            TotalAmount = facilityPlayer.TotalAmount,
                            IsFree = item.IsFree,
                            BookingId = item.BookingId,
                            Day = (DayOfWeek)item.Date.Day,
                            Start = item.PitchStart,
                            End = item.PitchStart,
                            Player = facilityPlayer,
                            BookingDate = item.Date,
                            Location = facility.Area.AreaName != null ? facility.Area.AreaName : String.Empty,
                            Facility = facility.Name,
                            PricePerPlayer = item.PricePerUser != null ? item.PricePerUser.Value : default,
                            Sport = sports.Where(s => s.SportId == item.SportId).FirstOrDefault() != null ? sports.Where(s => s.SportId == item.SportId).FirstOrDefault().Name : String.Empty
                        });
                    }
                }

            }

            return cancelPlayBookingVm;
        }


        public async Task<bool> DeleteBookingId(CancelPlayBookingViewModel bookingSlot)
        {
            var userPitchBooking = await context.UserPitchBookings.AsNoTracking().Where(f => f.BookingId == bookingSlot.BookingId).FirstOrDefaultAsync();
            if(userPitchBooking != null)
            {
                context.UserPitchBookings.Remove(userPitchBooking);
                var facilityPlayer = await context.FacilityPlayers.AsNoTracking().Where(f => f.BookingId == bookingSlot.BookingId).FirstOrDefaultAsync();
                if(facilityPlayer != null)
                {
                    facilityPlayer.BookingId = Guid.Empty;

                    facilityPlayer.LastEditedDate = DateTime.Now;

                    context.FacilityPlayers.Update(facilityPlayer);
                }
                await context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<IndividualBooking> GetIndividualBookingByTelRRefNo(string telRRefNo)
        {
            return await context.IndividualBookings
                .Where(x => x.TelRRefNo == telRRefNo)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetCount_IndividualBookingByDate(DateTime date)
        {
            return await context.IndividualBookings
                .Where(x => x.CreatedDate.Value.Date == date.Date)
                .CountAsync();
        }

        public async Task<Guid> InsertUpdateIndividualBooking(Guid userId,
            IndividualBooking booking)
        {
            var getBooking = await GetIndividualBooking(booking.BookingId);
            if (getBooking != null)
            {
                getBooking.Coaching = booking.Coaching;
                getBooking.Date = booking.Date;
                getBooking.StartTime = booking.StartTime;
                getBooking.EndTime = booking.EndTime;
                getBooking.Location = booking.Location;
                getBooking.Notes = booking.Notes;
                getBooking.LastEditedBy = userId;
                getBooking.LastEditedDate = Helper.GetDateTime();

                context.IndividualBookings.Update(getBooking);
            }
            else
            {
                booking.Status = Model.EBookingStatus.Pending;
                booking.BookingId = Guid.NewGuid();
                booking.TraineeId = userId;
                booking.CreatedBy = userId;
                booking.CreatedDate = Helper.GetDateTime();
                booking.IsEnabled = true;
                booking.DateEnabled = Helper.GetDateTime();
                context.IndividualBookings.Add(booking);
            }
            await context.SaveChangesAsync();
            return booking.BookingId;
        }



        public async Task<IEnumerable<CoachNotAvailableScheduleViewModel>> GetCoachNotAvailableSched(Guid coachId)
        {
            var dateNow = Helper.GetDateTime();
            var list = new List<CoachNotAvailableScheduleViewModel>();
            var query = await (from x in context.IndividualBookings
                               join y in context.IndividualClasses
                                on x.ClassId equals y.ClassId
                               where y.CoachId == coachId && x.Status == EBookingStatus.Approved
                               //&& x.Date.Date > dateNow.Date
                               select new CoachNotAvailableScheduleViewModel()
                               {
                                   Date = x.Date,
                                   StartTime = x.StartTime,
                                   EndTime = x.EndTime
                               })
                               .ToListAsync();
            list.AddRange(query);

            var querynotavailable = await (from x in context.CoachAbsences
                                           where x.CoachUserId == coachId
                                           select new CoachNotAvailableScheduleViewModel()
                                           {
                                               Date = x.EndDate,
                                               StartTime = x.StartTime,
                                               EndTime = x.EndTime
                                           }).ToListAsync();
            list.AddRange(querynotavailable);

            // do we need to include the groupclasses schedule????
            return list;
        }
        public async Task UpdateIndividualBooking(Guid userId,
            IndividualBooking booking)
        {
            if (booking != null)
            {
                booking.LastEditedBy = userId;
                booking.LastEditedDate = Helper.GetDateTime();
                context.IndividualBookings.Update(booking);
                await context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<GroupBookingViewModel>> GetGroupBookingsByUser(Guid userId, bool getLatest)
        {
            var query = (from x in context.GroupBookings
                         join y in context.GroupClasses
                           on x.GroupClassId equals y.GroupClassId
                         join participant in context.Users
                           on x.ParticipantId equals participant.UserId
                         join coach in context.Users
                           on y.CoachId equals coach.UserId
                         where x.ParticipantId == userId
                         select new GroupBookingViewModel
                         {
                             GroupBookingId = x.GroupBookingId,
                             GroupClassId = x.GroupClassId,
                             ParticipantId = x.ParticipantId,
                             Status = x.Status,
                             ParticipantFirstName = participant.FirstName,
                             ParticipantLastName = participant.LastName,
                             CoachFirstName = coach.FirstName,
                             CoachLastName = coach.LastName,
                             Title = y.Title,
                             ByLevel = y.ByLevel,
                             Level = y.LevelId,
                             Start = y.Start,
                             End = y.Start.GetValueOrDefault().AddHours(y.Duration.GetValueOrDefault()),
                             RepeatEveryWeek = y.RepeatEveryWeek,
                             DuringNo = y.DuringNo,
                             During = y.During,
                             Participants = y.Participants,
                             ParticipantsJoined = (context.GroupBookings.Count(c => c.Status == EBookingStatus.Approved
                                                                                 && c.GroupClassId == x.GroupClassId)),
                             Price = y.Price,
                             LocationId = y.LocationId,
                             IsOnline = y.IsOnline,
                             Notes = y.Notes,
                         });
            var dateNow = Helper.GetDateTime();
            if (getLatest)
            {
                return await query.Where(x => x.End >= dateNow)
                              .ToListAsync();
            }
            else
            {
                return await query.Where(x => x.End < dateNow)
                              .ToListAsync();
            }


        }

        public async Task<IEnumerable<GroupBookingViewModel>> GetGroupBookingsByCoach(Guid userId, bool getLatest)
        {
            var query = (from x in context.GroupBookings
                         join y in context.GroupClasses
                           on x.GroupClassId equals y.GroupClassId
                         join participant in context.Users
                           on x.ParticipantId equals participant.UserId
                         join coach in context.Users
                           on y.CoachId equals coach.UserId
                         where y.CoachId == userId
                         select new GroupBookingViewModel
                         {
                             GroupBookingId = x.GroupBookingId,
                             GroupClassId = x.GroupClassId,
                             ParticipantId = x.ParticipantId,
                             Status = x.Status,
                             ParticipantFirstName = participant.FirstName,
                             ParticipantLastName = participant.LastName,
                             CoachFirstName = coach.FirstName,
                             CoachLastName = coach.LastName,
                             Title = y.Title,
                             ByLevel = y.ByLevel,
                             Level = y.LevelId,
                             Start = y.Start,
                             End = y.Start.GetValueOrDefault().AddHours(y.Duration.GetValueOrDefault()),
                             RepeatEveryWeek = y.RepeatEveryWeek,
                             DuringNo = y.DuringNo,
                             During = y.During,
                             Participants = y.Participants,
                             ParticipantsJoined = (context.GroupBookings.Count(c => c.Status == EBookingStatus.Approved
                                                                                 && c.GroupClassId == x.GroupClassId)),
                             Price = y.Price,
                             LocationId = y.LocationId,
                             IsOnline = y.IsOnline,
                             Notes = y.Notes,
                         });
            var dateNow = Helper.GetDateTime();
            if (getLatest)
            {
                return await query.Where(x => x.End >= dateNow)
                              .ToListAsync();
            }
            else
            {
                return await query.Where(x => x.End < dateNow)
                              .ToListAsync();
            }


        }

        public async Task<GroupBookingViewModel> GetGroupBooking(Guid bookingId)
        {
            return await (from x in context.GroupBookings
                          join y in context.GroupClasses
                            on x.GroupClassId equals y.GroupClassId
                          join participant in context.Users
                            on x.ParticipantId equals participant.UserId
                          join coach in context.Users
                            on y.CoachId equals coach.UserId
                          join gym in context.Gyms
                            on y.GymId equals gym.GymId
                          where x.GroupBookingId == bookingId
                          select new GroupBookingViewModel
                          {
                              GroupBookingId = x.GroupBookingId,
                              GroupClassId = x.GroupClassId,
                              ParticipantId = x.ParticipantId,
                              Status = x.Status,
                              CoachImage = coach.ImageUrl,
                              ParticipantFirstName = participant.FirstName,
                              ParticipantLastName = participant.LastName,
                              CoachFirstName = coach.FirstName,
                              CoachLastName = coach.LastName,
                              Title = y.Title,
                              ByLevel = y.ByLevel,
                              Level = y.LevelId,
                              Start = y.Start,
                              StartTime = y.StartTime,
                              TotalAmount = x.TotalAmount,
                              SideKickCommission = x.SideKickCommission,
                              Date = x.Date.GetValueOrDefault().ToString("yyyy-MM-ddTHH:mm:ss"),
                              EndTime = !string.IsNullOrWhiteSpace(y.StartTime) ? TimeSpan.Parse(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, TimeSpan.Parse(y.StartTime).Hours, TimeSpan.Parse(y.StartTime).Minutes, 0).AddHours((int)y.Duration.GetValueOrDefault()).ToString("HH:mm")).ToString().Substring(0, 5):"",
                              Duration = (int)(y.Duration != null ? y.Duration : null),
                              End = y.Start.GetValueOrDefault().AddHours(y.Duration.GetValueOrDefault()),
                              RepeatEveryWeek = y.RepeatEveryWeek,
                              DuringNo = y.DuringNo,
                              During = y.During,
                              Participants = y.Participants,
                              Price = y.Price,
                              LocationId = y.LocationId,
                              IsOnline = y.IsOnline,
                              Notes = y.Notes,
                              GymLocationName = gym.GymName,
                              LocationName = gym.GymAddress,
                              TransactionNo = x.TransactionNo
                          })
                          .FirstOrDefaultAsync();
        }

        public async Task<int> GetCount_GroupBookingByDate(DateTime date)
        {
            return await context.GroupBookings
                .Where(x => x.CreatedDate.Value.Date == date.Date)
                .CountAsync();
        }

        public async Task<GroupBooking> GetGroupBookingDb(Guid bookingId)
        {
            return await context.GroupBookings
                .Where(x => x.GroupBookingId == bookingId)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<GroupBooking>> GetGroupBookingPerGroupClass(Guid groupClassId)
        {
            return await context.GroupBookings
                .Where(x => x.GroupClassId == groupClassId)
                .ToListAsync();
        }

        public async Task UpdateGroupBooking(Guid userId,
            GroupBooking booking)
        {
            if (booking != null)
            {
                booking.LastEditedBy = userId;
                booking.LastEditedDate = Helper.GetDateTime();
                context.GroupBookings.Update(booking);
                await context.SaveChangesAsync();
            }
        }

        public async Task<Guid> InsertGroupBooking(Guid userId,
            GroupBooking booking)
        {
            if (booking != null)
            {
                booking.Status = EBookingStatus.Pending;
                booking.GroupBookingId = Guid.NewGuid();
                booking.CreatedBy = userId;
                booking.CreatedDate = Helper.GetDateTime();
                booking.IsEnabled = true;
                booking.DateEnabled = Helper.GetDateTime();
                context.GroupBookings.Add(booking);
                await context.SaveChangesAsync();
                return booking.GroupBookingId;
            }
            return Guid.Empty;
        }

        public async Task<IEnumerable<BookingViewModel>> GetAllBookingBeforeSetTimeOfAppointment(DateTime dateTime)
        {
            var list = new List<BookingViewModel>();
            var individual = await context.IndividualBookings
                .Where(x => x.IsPaymentValidated == true && x.Status == EBookingStatus.Confirmed)
                .Select(x => new BookingViewModel()
                {
                    BookingId = x.BookingId,
                    BookingType = EBookingType.Individual,
                    diff = (x.Date - dateTime).TotalSeconds
                })
                .ToListAsync();

            var group = await (from x in context.GroupBookings
                               join y in context.GroupClasses
                                on x.GroupClassId equals y.GroupClassId
                               where y.Start.Value.Date <= dateTime.Date
                               && x.IsPaymentValidated == false && x.Status == EBookingStatus.Confirmed
                               select new BookingViewModel()
                               {
                                   BookingId = x.GroupBookingId,
                                   BookingType = EBookingType.Group,
                                   diff = (x.Date.Value - dateTime).TotalSeconds
                               })
                               .ToListAsync();

            list.AddRange(individual.Where(a => a.diff < 300));
            list.AddRange(group.Where(a => a.diff < 300));
            return list;
        }


        public async Task<IEnumerable<BookingViewModel>> GetAllBookingBefore48HoursSetTimeOfAppointment(DateTime dateTime)
        {
            var list = new List<BookingViewModel>();
            var individual = await context.IndividualBookings.Where(x => x.IsPaymentValidated == false && x.Status == EBookingStatus.Confirmed)
                .Select(x => new BookingViewModel()
                {
                    BookingId = x.BookingId,
                    BookingType = EBookingType.Individual,
                    diff = (x.Date - dateTime).TotalSeconds
                }).ToListAsync();

            var group = await (from x in context.GroupBookings
                               join y in context.GroupClasses
                                on x.GroupClassId equals y.GroupClassId
                               where x.IsPaymentValidated == false && x.Status == EBookingStatus.Confirmed
                               select new BookingViewModel()
                               {
                                   BookingId = x.GroupBookingId,
                                   BookingType = EBookingType.Group,
                                   diff = (x.Date.Value - dateTime).TotalSeconds
                               })
                               .ToListAsync();

            list.AddRange(individual.Where(a => a.diff < 172800));
            list.AddRange(group.Where(a => a.diff < 172800));
            return list;
        }

        public async Task<GroupBookingViewModel> GetGroupBookingByTelRRefNo(string telRRefNo)
        {
            return await (from x in context.GroupBookings
                          join y in context.GroupClasses
                            on x.GroupClassId equals y.GroupClassId
                          join participant in context.Users
                            on x.ParticipantId equals participant.UserId
                          join coach in context.Users
                            on y.CoachId equals coach.UserId
                          join gym in context.Gyms
                            on y.GymId equals gym.GymId
                          where x.TelRRefNo == telRRefNo
                          select new GroupBookingViewModel
                          {
                              GroupBookingId = x.GroupBookingId,
                              GroupClassId = x.GroupClassId,
                              ParticipantId = x.ParticipantId,
                              Status = x.Status,
                              ParticipantFirstName = participant.FirstName,
                              ParticipantLastName = participant.LastName,
                              CoachFirstName = coach.FirstName,
                              CoachLastName = coach.LastName,
                              Title = y.Title,
                              ByLevel = y.ByLevel,
                              Level = y.LevelId,
                              Start = y.Start,
                              StartTime = y.StartTime,
                              TotalAmount = x.TotalAmount,
                              SideKickCommission = x.SideKickCommission,
                              Date = x.Date.GetValueOrDefault().ToString("yyyy-MM-ddTHH:mm:ss"),
                              EndTime =!string.IsNullOrWhiteSpace(y.StartTime)? TimeSpan.Parse(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, TimeSpan.Parse(y.StartTime).Hours, TimeSpan.Parse(y.StartTime).Minutes, 0).AddHours((int)y.Duration.GetValueOrDefault()).ToString("HH:mm")).ToString().Substring(0, 5):"",
                              Duration = (int)(y.Duration != null ? y.Duration : null),
                              End = y.Start.GetValueOrDefault().AddHours(y.Duration.GetValueOrDefault()),
                              RepeatEveryWeek = y.RepeatEveryWeek,
                              DuringNo = y.DuringNo,
                              During = y.During,
                              Participants = y.Participants,
                              Price = y.Price,
                              LocationId = y.LocationId,
                              IsOnline = y.IsOnline,
                              Notes = y.Notes,
                              GymLocationName = gym.GymName,
                              LocationName = gym.GymAddress
                          })
                          .FirstOrDefaultAsync();
        }

        public async Task<List<TrainingListForPushNotificationViewModel>> GetIndividualClassListForPushNotification(DateTime startDate, DateTime endDate)
        {
            var group = await (from x in context.IndividualBookings
                               join y in context.IndividualClasses on x.ClassId equals y.ClassId
                               join u in context.Users on y.CoachId equals u.UserId
                               where x.IsPaid == true && x.Date >= startDate && x.Date <= endDate
                               select new TrainingListForPushNotificationViewModel()
                               {
                                   BookingId = x.BookingId,
                                   BookingDate = x.Date,
                                   UserId = x.TraineeId,
                                   CoachId = y.CoachId,
                                   CoachName = u.FirstName + " " + u.LastName,
                                   StartTime = x.StartTime
                               }).ToListAsync();
            return group;
        }

        public async Task<List<TrainingListForPushNotificationViewModel>> GetGroupClassListForPushNotification(DateTime startDate, DateTime endDate)
        {
            var group = await (from x in context.GroupBookings
                               join y in context.GroupClasses on x.GroupClassId equals y.GroupClassId
                               join u in context.Users on y.CoachId equals u.UserId
                               where y.Start.HasValue && y.Start.Value >= startDate && y.Start <= endDate
                                && x.IsPaid == true
                               select new TrainingListForPushNotificationViewModel()
                               {
                                   BookingId = x.GroupBookingId,
                                   BookingDate = y.Start.Value,
                                   UserId = x.ParticipantId,
                                   CoachId = y.CoachId,
                                   CoachName = u.FirstName + " " + u.LastName
                               }).ToListAsync();
            return group;
        }

       
    }
}
