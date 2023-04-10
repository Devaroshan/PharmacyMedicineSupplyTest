using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NuGet.Protocol.Plugins;
using PharmacyMedicineSupply.Controllers;
using PharmacyMedicineSupply.Models;
using PharmacyMedicineSupply.Models.DTO.MedicalRepresentative;
using PharmacyMedicineSupply.Models.DTO.MedicineStock;
using PharmacyMedicineSupply.Models.DTO.MedicineSupply;
using PharmacyMedicineSupply.Models.DTO.PharmacyMedSupply;
using PharmacyMedicineSupply.Repository;
using PharmacyMedicineSupply.Repository.EntityClasses;
using PharmacySupplyProject.Models;
using System.Collections.Generic;
using System.Net.Sockets;

namespace PharmacyMedicineSupplyTest
{
    public class Tests
    {
        string date;
        DatesScheduleController _datescontroller;
        ManagerController _managercontroller;
        MedicalRepresentativeScheduleController _RepSchController;
        MedicineStocksController _MedStockController;
        MedicineDemandController _MedDemandController;
        PharmacyMedSupplyController _pharMedSupController;        
        Mock<IUnitOfWork> _unitOfWork;
        private List<MedicineDemand> Demands ;
        private List<DatesSchedule> DatesSch;
        private List<DatesSchedule> DatesSchEmpty;
        private List<RepresentativeSchedule> RepSch;
        private List<MedicineStockDTO> MedStock;
        private List<PharmacyMedSupplyDTO> PharMedSup;
        MedicineStockResponse MedStockRes;
        PharmacyMedSupplyResponse PharMedSupRes;
        MedicineDemand demand;
        Manager manager;
        IQueryable<MedicineDemand> DemandData;
        Mock<DbSet<MedicineDemand>> DemandsMock;
        Mock<PharmacySupplyContext> mockPharmacyContext;
        MedicineDemandRepository MedicineDemandRepo;
        [SetUp]
        public void Setup()
        {
            PharMedSupRes = new PharmacyMedSupplyResponse();
            MedStockRes = new MedicineStockResponse();
            manager = new Manager()
            {
                Name = "man",
                Email = "a@gmail.com",
                Password = "password"
            };
            PharMedSup = new List<PharmacyMedSupplyDTO> { new PharmacyMedSupplyDTO() };
            MedStock = new List<MedicineStockDTO> { new MedicineStockDTO() };
            RepSch = new List<RepresentativeSchedule>(){ new RepresentativeSchedule() };
            date = "01-01-2023";
            DatesSchEmpty = new List<DatesSchedule>();
            _unitOfWork = new Mock<IUnitOfWork>();
            _datescontroller = new DatesScheduleController(_unitOfWork.Object);
            _managercontroller = new ManagerController(_unitOfWork.Object);
            _RepSchController = new MedicalRepresentativeScheduleController(_unitOfWork.Object);
            _MedStockController = new MedicineStocksController(_unitOfWork.Object);
            _MedDemandController = new MedicineDemandController(_unitOfWork.Object);
            _pharMedSupController = new PharmacyMedSupplyController(_unitOfWork.Object);
            demand = new MedicineDemand()
            {
                Name = "MRR",
                DemandCount = 10
            };
            Demands = new List<MedicineDemand>() {
            new MedicineDemand
            {
            Name= "MPP",
            DemandCount = 10
            },
            new MedicineDemand
            {
            Name= "MRP",
            DemandCount = 15
            }
            };
            DatesSch = new List<DatesSchedule>() {
            new DatesSchedule
            {
            StartDate= new DateTime(2019,05,09,9,15,0),
            EndDate = new DateTime(2019,05,19,9,15,0),
            Completed = 0,
            Supplied = 0,
            CountCompletedMeets = 2
            },
            new DatesSchedule
            {
            StartDate= new DateTime(2019,06,09,9,15,0),
            EndDate = new DateTime(2019,06,19,9,15,0),
            Completed = 1,
            Supplied = 0,
            CountCompletedMeets = 2
            }
            };
            DemandData = Demands.AsQueryable();
            DemandsMock = new Mock<DbSet<MedicineDemand>>();
            DemandsMock.As<IQueryable<MedicineDemand>>().Setup(m => m.Provider).Returns(DemandData.Provider);
            DemandsMock.As<IQueryable<MedicineDemand>>().Setup(m => m.Expression).Returns(DemandData.Expression);
            DemandsMock.As<IQueryable<MedicineDemand>>().Setup(m => m.ElementType).Returns(DemandData.ElementType);
            DemandsMock.As<IQueryable<MedicineDemand>>().Setup(m => m.GetEnumerator()).Returns(DemandData.GetEnumerator());
            var p = new DbContextOptions<PharmacySupplyContext>();
            mockPharmacyContext = new Mock<PharmacySupplyContext>(p);
            MedicineDemandRepo = new MedicineDemandRepository(mockPharmacyContext.Object);
        }
        [Test]
        public void GetMedicineDemand()
        {
            mockPharmacyContext.Setup(x => x.MedicineDemands).Returns(DemandsMock.Object);
            var result = MedicineDemandRepo.GetMedicineDemand();
            Assert.That(result, Is.InstanceOf<Task<List<MedicineDemand>>>());
        }

        [Test]
        public void AddMedicineDemand()
        {
            mockPharmacyContext.Setup(x => x.MedicineDemands).Returns(DemandsMock.Object);
            var result = MedicineDemandRepo.AddMedicineDemand(demand);
            Assert.That(result, Is.InstanceOf<Task<MedicineDemand>>());
        }

        [Test]
        public void UpdateMedicineDemand()
        {
            mockPharmacyContext.Setup(x => x.MedicineDemands).Returns(DemandsMock.Object);
            var result = MedicineDemandRepo.UpdateMedicineDemand("MPP",10);
            Assert.That(result, Is.InstanceOf<Task<MedicineDemand>>());
        }
        [Test]
        public void GetAllDatesSchedule()
        {
            _unitOfWork.Setup(p => p.DatesScheduleRepository.GetAllDatesScheduled()).Returns(Task.FromResult(DatesSch));
            var actionResult = _datescontroller.GetAllDatesScheduled();
            // Assert
            Assert.That(actionResult, Is.InstanceOf<Task<ActionResult<IEnumerable<DatesSchedule>>>>());
        }
        [Test]
        public void CheckDateAvailability()
        {
            DateTime selectedDate = Convert.ToDateTime(date);
            _unitOfWork.Setup(p => p.DatesScheduleRepository.CheckAvailability(selectedDate)).Returns(Task.FromResult(true));
            var actionResult = _datescontroller.DateAvailable(date);
            Assert.That(actionResult, Is.InstanceOf<Task<ActionResult<bool>>>());
            Assert.AreEqual(actionResult.Result.Value, true);
        }
        [Test]
        public void CheckNegativeDateAvailability()
        {
            DateTime selectedDate = Convert.ToDateTime(date);
            _unitOfWork.Setup(p => p.DatesScheduleRepository.CheckAvailability(selectedDate)).Returns(Task.FromResult(false));
            var actionResult = _datescontroller.DateAvailable(date);
            Assert.That(actionResult, Is.InstanceOf<Task<ActionResult<bool>>>());
            Assert.AreEqual(actionResult.Result.Value,false);
        }

        [Test]
        public void GetScheduleByDate()
        {
            DateTime selectedDate = Convert.ToDateTime(date);
            _unitOfWork.Setup(p => p.RepresentativeScheduleRepository.GetScheduleByDate(selectedDate)).Returns(Task.FromResult(RepSch));
            var actionResult = _RepSchController.GetScheduleByDate(date);
            Assert.That(actionResult, Is.InstanceOf<Task<ActionResult<IEnumerable<RepresentativeSchedule>>>>());
        }
        [Test]
        public void MedicineStockInformation()
        {
            _unitOfWork.Setup(p => p.MedicineStockRepository.GetMedicineStocks(2)).Returns(Task.FromResult(MedStockRes));
            var actionResult = _MedStockController.MedicineStockInformation(2);
            Assert.That(actionResult, Is.InstanceOf<Task<ActionResult<MedicineStockResponse>>>());
        }
        [Test]
        public void UpdateMedicineDemandController()
        {
            _unitOfWork.Setup(p => p.MedicineDemandRepository.UpdateMedicineDemand("MPP",10)).Returns(Task.FromResult(demand));
            var actionResult = _MedDemandController.UpdateMedicneDemand("MPP",10);
            Assert.That(actionResult, Is.InstanceOf<Task<ActionResult<MedicineDemand>>>());
        }
        [Test]
        public void GetAlreadySuppliedPharma()
        {
            DateTime selectedDate = Convert.ToDateTime(date);
            _unitOfWork.Setup(p => p.PharmacyMedSupplyRepository.GetPharmacyMedicineSupplyByDate(1,selectedDate)).Returns(Task.FromResult(PharMedSupRes));
            var actionResult = _pharMedSupController.GetAlreadySuppliedPharma(1,date);
            Assert.That(actionResult, Is.InstanceOf<Task<ActionResult<PharmacyMedSupplyResponse>>>());
        }
        [Test]
        public void CheckManagerEmail()
        {
            _unitOfWork.Setup(p => p.ManagerRepository.GetManagerbymail("")).Returns(Task.FromResult(manager));
            var actionResult = _managercontroller.CheckManagerEmail("");
            Assert.That(actionResult.Result, Is.InstanceOf<OkObjectResult>());
            Assert.AreEqual(actionResult.Result.Equals(false), false);
        }
        [Test]
        public void NegativeCheckManagerEmail()
        {
            var actionResult = _managercontroller.CheckManagerEmail("");
            Assert.That(actionResult.Result, Is.InstanceOf<BadRequestObjectResult>());
        }
    }
}