﻿// Copyright (c) 2018 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using DataLayer.Dtos;
using DataLayer.EfClasses;
using DataLayer.EfCode;
using TestNetStandard2_0Only.Helpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace TestNetStandard2_0Only.UnitTests.DDDEntities
{
    public class TestDddOrder
    {
        [Fact]
        public void TestCreateOrderWithCorrectLineNumsOk()
        {
            //SETUP
            var book1 = DddEfTestData.CreateDummyBookOneAuthor();
            var book2 = DddEfTestData.CreateDummyBookOneAuthor();

            //ATTEMPT
            var bookOrders = new List<OrderBooksDto>() { new OrderBooksDto(book1.BookId, book1, 1), new OrderBooksDto(book2.BookId, book2, 2) };
            var status = Order.CreateOrderFactory("user", DateTime.Today.AddDays(3), bookOrders);

            //VERIFY
            status.HasErrors.ShouldBeFalse();
            status.Result.LineItems.Count().ShouldEqual(2);
            status.Result.LineItems.First().LineNum.ShouldEqual((byte)1);
            status.Result.LineItems.Last().LineNum.ShouldEqual((byte)2);
        }

        [Fact]
        public void TestCreateOrderCorrectBookInfoOk()
        {
            //SETUP
            var book = DddEfTestData.CreateDummyBookOneAuthor();

            //ATTEMPT
            var lineItems = new List<OrderBooksDto> { new OrderBooksDto(book.BookId, book, 3) };
            var status = Order.CreateOrderFactory("user", DateTime.Today.AddDays(3), lineItems);

            //VERIFY
            status.HasErrors.ShouldBeFalse();
            status.Result.LineItems.Count().ShouldEqual(1);
            status.Result.LineItems.First().NumBooks.ShouldEqual((short)3);
            status.Result.LineItems.First().BookPrice.ShouldEqual(book.ActualPrice);
        }

        [Fact]
        public void TestCreateOrderNoLineItemsOk()
        {
            //SETUP

            //ATTEMPT
            var status = Order.CreateOrderFactory("user", DateTime.Today.AddDays(3), new OrderBooksDto[]{});

            //VERIFY
            status.HasErrors.ShouldBeTrue();
            status.Errors.Single().ErrorResult.ErrorMessage.ShouldEqual("No items in your basket.");
        }

        [Fact]
        public void TestChangeDeliveryDateOk()
        {
            //SETUP
            var book = DddEfTestData.CreateDummyBookOneAuthor();
            var lineItems = new List<OrderBooksDto> { new OrderBooksDto(book.BookId, book, 3) };
            var order = Order.CreateOrderFactory("user", DateTime.Today.AddDays(1), lineItems).Result;

            //ATTEMPT
            var newDeliverDate = DateTime.Today.AddDays(2);
            if (newDeliverDate.DayOfWeek == DayOfWeek.Sunday)
                newDeliverDate = newDeliverDate.AddDays(1);
            var status = order.ChangeDeliveryDate("user", newDeliverDate);

            //VERIFY
            status.HasErrors.ShouldBeFalse();
            order.ExpectedDeliveryDate.ShouldEqual(newDeliverDate);
        }

        [Fact]
        public void TestCreateOrderAndAddToDbOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<EfCoreContext>();
            using (var context = new EfCoreContext(options))
            {
                context.Database.EnsureCreated();
                context.SeedDatabaseFourBooks();
            }

            using (var context = new EfCoreContext(options))
            {
                //ATTEMPT
                var book = context.Books.First();
                var lineItems = new List<OrderBooksDto> { new OrderBooksDto(book.BookId, book, 1) };
                context.Add(Order.CreateOrderFactory("user", DateTime.Today.AddDays(3), lineItems).Result);
                context.SaveChanges();

                //VERIFY
                context.Orders.Count().ShouldEqual(1);
                context.Set<LineItem>().Count().ShouldEqual(1);
            }
        }

        

    }

}