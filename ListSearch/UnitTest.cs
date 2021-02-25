using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace ListSearch
{

    public class Tests
    {
        ListService _ListService;
        IQueryable<ModelA> _TastDatas; 
        [SetUp]
        public void Setup()
        {
            _ListService = new ListService();
            _TastDatas = new List<ModelA>()
            {
                new ModelA()
                {
                    Id = 1, Name ="A", SonModelA = new List<ModelA>(){ new ModelA() { Id = 1001, Name = "A1"},new ModelA() {Id = 1002,Name ="A2"  } }
                },
                new ModelA()
                {
                    Id = 2, Name ="B", SonModelA = new List<ModelA>(){ new ModelA() { Id = 2001, Name = "B1"}}
                },
                new ModelA()
                {
                    Id = 3, Name ="C"
                },
            }.AsQueryable();
        }

        [Test]
        //分頁列表_第二列第一筆_查詢正確版
        public void Test_分頁列表_第二列第一筆_查詢正確版()
        {
            //arrange
            var expected = new List<ModelA>() 
            {
                new ModelA()
                {
                    Id = 2, Name ="B", SonModelA = new List<ModelA>(){ new ModelA() { Id = 2001, Name = "B1"}}
                } 
            };
            //act
            var act = _ListService.ToPage(_TastDatas, 2 ,1).ToList();
            //assert
            Assert.AreEqual(expected,act);

        }
        [Test]
        //分頁列表_第二列第一筆_查詢錯誤版
        public void Test_分頁列表_第二列第一筆_查詢錯誤版()
        {
            //arrange
            var expected = new List<ModelA>()
            {
                new ModelA()
                {
                    Id = 2, Name ="B"
                }
            };
            //act
            var act = _ListService.ToPage(_TastDatas, 2, 1);
            //assert
            Assert.AreNotEqual(expected, act);

        }
        [TestCase("Id")]
        [TestCase("Name")]
        //Test_排序列表_針對欄位降序
        public void Test_排序列表_針對欄位降序(string orderName)
        {
            //arrange
            var expected = new List<ModelA>()
            {
                new ModelA()
                {
                    Id = 3, Name ="C"
                },
                new ModelA()
                {
                    Id = 2, Name ="B", SonModelA = new List<ModelA>(){ new ModelA() { Id = 2001, Name = "B1"}}
                },
                new ModelA()
                {
                    Id = 1, Name ="A", SonModelA = new List<ModelA>(){ new ModelA() { Id = 1001, Name = "A1"},new ModelA() {Id = 1002,Name ="A2"  } }
                }
            };
            //act
            var act = _ListService.ToOrder(_TastDatas, orderName, true);
            //assert
            Assert.AreEqual(expected, act);
        }
        
        [Test]
        //查詢列表_查詢字串1
        public void Test_查詢列表_查詢字串1()
        {
            //arrange
            var expected = new List<ModelA>()
            {
                new ModelA()
                {
                    Id = 1, Name ="A", SonModelA = new List<ModelA>(){ new ModelA() { Id = 1001, Name = "A1"},new ModelA() {Id = 1002,Name ="A2"  } }
                },
                new ModelA()
                {
                    Id = 2, Name ="B", SonModelA = new List<ModelA>(){ new ModelA() { Id = 2001, Name = "B1"}}
                }
            }.AsQueryable();
            //act
            var act = _ListService.SearchString(_TastDatas, "1");
            //assert
            Assert.AreEqual(expected, act);
        }
        [Test]
        //查詢列表_查詢_1_忽略SonModelA
        public void Test_查詢列表_查詢_1_忽略SonModelA()
        {
            //arrange
            var expected = new List<ModelA>()
            {
                new ModelA()
                {
                    Id = 1, Name ="A", SonModelA = new List<ModelA>(){ new ModelA() { Id = 1001, Name = "A1"},new ModelA() {Id = 1002,Name ="A2"  } }
                }
            }.AsQueryable();
            //act
            var act = _ListService.SearchString(_TastDatas, "1"," ",new List<string>() { "SonModelA"});
            //assert
            Assert.AreEqual(expected, act);
        }
        [Test]
        //查詢列表_查詢_1_階層查詢到0
        public void Test_查詢列表_查詢_1_階層查詢到0()
        {
            //arrange
            var expected = new List<ModelA>()
            {
                new ModelA()
                {
                    Id = 1, Name ="A", SonModelA = new List<ModelA>(){ new ModelA() { Id = 1001, Name = "A1"},new ModelA() {Id = 1002,Name ="A2"  } }
                }
            }.AsQueryable();
            //act
            var act = _ListService.SearchString(_TastDatas, "1", " ",null,0);
            //assert
            Assert.AreEqual(expected, act);
        }

        [TestCase("1 C"," ")]
        [TestCase("1_C","_")]
        //查詢列表_查詢_1跟C_忽略SonModelA
        public void Test_查詢列表_查詢_1跟C_忽略SonModelA(string search,string breakString)
        {
            //arrange
            var expected = new List<ModelA>()
            {
                new ModelA()
                {
                    Id = 1, Name ="A", SonModelA = new List<ModelA>(){ new ModelA() { Id = 1001, Name = "A1"},new ModelA() {Id = 1002,Name ="A2"  } }
                }  ,
                new ModelA()
                {
                    Id = 3, Name ="C"
                },
            }.AsQueryable();
            //act
            var act = _ListService.SearchString(_TastDatas, search, breakString, new List<string>() { "SonModelA" });
            //assert
            Assert.AreEqual(expected, act);
        }
        //集合所有功能
        public void Test_集合所有功能() 
        {
            //arrange
            var expected = new List<ModelA>()
            {
                new ModelA()
                {
                    Id = 3, Name ="C"
                } ,
                new ModelA()
                {
                    Id = 1, Name ="A", SonModelA = new List<ModelA>(){ new ModelA() { Id = 1001, Name = "A1"},new ModelA() {Id = 1002,Name ="A2"  } }
                } ,
            }.AsQueryable();
            //act
            var act = _ListService.ToPage(
                            _ListService.ToOrder(
                                _ListService.SearchString(_TastDatas, "1 C", " ", new List<string>() { "SonModelA" }),
                                   "Id", true)
                            ,1,2);
            //assert
            Assert.AreEqual(expected, act);
        }
    }
}