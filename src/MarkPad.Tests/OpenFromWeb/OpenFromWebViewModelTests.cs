﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Caliburn.Micro;
using MarkPad.OpenFromWeb;
using MarkPad.Services.Interfaces;
using MarkPad.Services.Metaweblog;
using MarkPad.Services.Settings;
using NSubstitute;
using Xunit;

namespace MarkPad.Tests.OpenFromWeb
{
    public class OpenFromWebViewModelTests
    {
        readonly OpenFromWebViewModel subject;
        
        readonly IMetaWeblogService metaWeblogService = Substitute.For<IMetaWeblogService>();
        readonly IDialogService dialogService = Substitute.For<IDialogService>();
        readonly ITaskSchedulerFactory taskScheduler = Substitute.For<ITaskSchedulerFactory>();

        public OpenFromWebViewModelTests()
        {
            taskScheduler.Current.Returns(TaskScheduler.Default);

            subject = new OpenFromWebViewModel(dialogService, s => metaWeblogService, taskScheduler);
        }

        [Fact]
        public void InitializeBlogs_WithNoBlogs_HasNoData()
        {
            subject.InitializeBlogs(new List<BlogSetting>());

            Assert.Empty(subject.Blogs);
            Assert.Null(subject.SelectedBlog);
        }

        [Fact]
        public void Cancel_Always_ReturnsFalse()
        {
            var conductor = Substitute.For<IConductor>();

            subject.Parent = conductor;

            subject.Cancel();

            conductor.Received().CloseItem(subject);
        }

        [Fact]
        public void CanFetch_WithNoBlogs_ReturnsFalse()
        {
            subject.InitializeBlogs(new List<BlogSetting>());

            Assert.False(subject.CanFetch);
        }

        [Fact]
        public void CanFetch_WithOneOrMoreBlogs_ReturnsTrue()
        {
            subject.InitializeBlogs(new List<BlogSetting> { new BlogSetting() });

            Assert.True(subject.CanFetch);
        }

        [Fact]
        public void CanContinue_AfterFetchingNoPosts_ReturnsFalse()
        {
            subject.InitializeBlogs(new List<BlogSetting> { new BlogSetting() });

            metaWeblogService
                .GetRecentPostsAsync(Arg.Any<BlogSetting>(), Arg.Any<int>())
                .Returns(Task<Post[]>.Factory.StartNew(() => new Post[0]));

            subject.Fetch()
                .Wait();

            Assert.Empty(subject.Posts);
            Assert.False(subject.CanContinue);
        }

        [Fact]
        public void CurrentPost_AfterFetchingOnePost_SelectsFirstPost()
        {
            subject.InitializeBlogs(new List<BlogSetting> { new BlogSetting() });

            var posts = new[] {new Post { title = "ABC"}};

            metaWeblogService
                .GetRecentPostsAsync(Arg.Any<BlogSetting>(), Arg.Any<int>())
                .Returns(Task<Post[]>.Factory.StartNew(() => posts));

            subject.Fetch()
                .Wait();

            Assert.NotEmpty(subject.Posts);
            Assert.Equal("ABC", subject.CurrentPost.Key);
        }

        [Fact]
        public void CanContinue_AfterFetchingOnePost_ReturnsTrue()
        {
            subject.InitializeBlogs(new List<BlogSetting> { new BlogSetting() });

            var posts = new[] { new Post { title = "ABC" } };

            metaWeblogService
                .GetRecentPostsAsync(Arg.Any<BlogSetting>(), Arg.Any<int>())
                .Returns(Task<Post[]>.Factory.StartNew(() => posts));

            subject.Fetch()
                .Wait();

            Assert.NotEmpty(subject.Posts);
            Assert.True(subject.CanContinue);
        }

        [Fact]
        public void Continue_WhenPostSelected_ReturnsTrue()
        {
            var conductor = Substitute.For<IConductor>();
            subject.Parent = conductor;

            subject.InitializeBlogs(new List<BlogSetting> { new BlogSetting() });

            var posts = new[] { new Post { title = "ABC" } };

            metaWeblogService
                .GetRecentPostsAsync(Arg.Any<BlogSetting>(), Arg.Any<int>())
                .Returns(Task<Post[]>.Factory.StartNew(() => posts));

            subject.Fetch()
                .Wait();

            subject.Continue();

            conductor.Received().CloseItem(subject);

            // TODO: get result from dialog
        }
    }
}
