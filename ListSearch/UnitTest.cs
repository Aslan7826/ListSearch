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
        //�����C��_�ĤG�C�Ĥ@��_�d�ߥ��T��
        public void Test_�����C��_�ĤG�C�Ĥ@��_�d�ߥ��T��()
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
        //�����C��_�ĤG�C�Ĥ@��_�d�߿��~��
        public void Test_�����C��_�ĤG�C�Ĥ@��_�d�߿��~��()
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
        //Test_�ƧǦC��_�w����쭰��
        public void Test_�ƧǦC��_�w����쭰��(string orderName)
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
        //�d�ߦC��_�d�ߦr��1
        public void Test_�d�ߦC��_�d�ߦr��1()
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
        //�d�ߦC��_�d��_1_����SonModelA
        public void Test_�d�ߦC��_�d��_1_����SonModelA()
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
        //�d�ߦC��_�d��_1_���h�d�ߨ�0
        public void Test_�d�ߦC��_�d��_1_���h�d�ߨ�0()
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
        //�d�ߦC��_�d��_1��C_����SonModelA
        public void Test_�d�ߦC��_�d��_1��C_����SonModelA(string search,string breakString)
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
        //���X�Ҧ��\��
        public void Test_���X�Ҧ��\��() 
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