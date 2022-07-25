using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.ImportLists.Spotify;
using NzbDrone.Core.Test.Framework;
using SpotifyAPI.Web;

namespace NzbDrone.Core.Test.ImportListTests
{
    [TestFixture]
    public class SpotifySavedAlbumsFixture : CoreTest<SpotifySavedAlbums>
    {
        // placeholder, we don't use real API
        private readonly SpotifyClient _api = null;

        [Test]
        public void should_not_throw_if_saved_albums_is_null()
        {
            Mocker.GetMock<ISpotifyProxy>().
                Setup(x => x.GetSavedAlbums(It.IsAny<SpotifySavedAlbums>(),
                                                It.IsAny<SpotifyClient>()))
                .Returns(default(Paging<SavedAlbum>));

            var result = Subject.Fetch(_api);

            result.Should().BeEmpty();
        }

        [Test]
        public void should_not_throw_if_saved_album_items_is_null()
        {
            var savedAlbums = new Paging<SavedAlbum>
            {
                Items = null
            };

            Mocker.GetMock<ISpotifyProxy>().
                Setup(x => x.GetSavedAlbums(It.IsAny<SpotifySavedAlbums>(),
                                                It.IsAny<SpotifyClient>()))
                .Returns(savedAlbums);

            var result = Subject.Fetch(_api);

            result.Should().BeEmpty();
        }

        [Test]
        public void should_not_throw_if_saved_album_is_null()
        {
            var savedAlbums = new Paging<SavedAlbum>
            {
                Items = new List<SavedAlbum>
                {
                    null
                }
            };

            Mocker.GetMock<ISpotifyProxy>().
                Setup(x => x.GetSavedAlbums(It.IsAny<SpotifySavedAlbums>(),
                                                It.IsAny<SpotifyClient>()))
                .Returns(savedAlbums);

            var result = Subject.Fetch(_api);

            result.Should().BeEmpty();
        }

        [TestCase("Artist", "Album")]
        public void should_parse_saved_album(string artistName, string albumName)
        {
            var savedAlbums = new Paging<SavedAlbum>
            {
                Items = new List<SavedAlbum>
                {
                    new SavedAlbum
                    {
                        Album = new FullAlbum
                        {
                            Name = albumName,
                            Artists = new List<SimpleArtist>
                            {
                                new SimpleArtist
                                {
                                    Name = artistName
                                }
                            }
                        }
                    }
                }
            };

            Mocker.GetMock<ISpotifyProxy>().
                Setup(x => x.GetSavedAlbums(It.IsAny<SpotifySavedAlbums>(),
                                                It.IsAny<SpotifyClient>()))
                .Returns(savedAlbums);

            var result = Subject.Fetch(_api);

            result.Should().HaveCount(1);
        }

        [Test]
        public void should_not_throw_if_get_next_page_returns_null()
        {
            var savedAlbums = new Paging<SavedAlbum>
            {
                Items = new List<SavedAlbum>
                {
                    new SavedAlbum
                    {
                        Album = new FullAlbum
                        {
                            Name = "Album",
                            Artists = new List<SimpleArtist>
                            {
                                new SimpleArtist
                                {
                                    Name = "Artist"
                                }
                            }
                        }
                    }
                },
                Next = "DummyToMakeHasNextTrue"
            };

            Mocker.GetMock<ISpotifyProxy>().
                Setup(x => x.GetSavedAlbums(It.IsAny<SpotifySavedAlbums>(),
                                                It.IsAny<SpotifyClient>()))
                .Returns(savedAlbums);

            Mocker.GetMock<ISpotifyProxy>()
                .Setup(x => x.GetNextPage(It.IsAny<SpotifyFollowedArtists>(),
                                          It.IsAny<SpotifyClient>(),
                                          It.IsAny<Paging<SavedAlbum>>()))
                .Returns(default(Paging<SavedAlbum>));

            var result = Subject.Fetch(_api);

            result.Should().HaveCount(1);

            Mocker.GetMock<ISpotifyProxy>()
                .Verify(x => x.GetNextPage(It.IsAny<SpotifySavedAlbums>(),
                                           It.IsAny<SpotifyClient>(),
                                           It.IsAny<Paging<SavedAlbum>>()),
                        Times.Once());
        }

        [TestCase(null, "Album")]
        [TestCase("Artist", null)]
        [TestCase(null, null)]
        public void should_skip_bad_artist_or_album_names(string artistName, string albumName)
        {
            var savedAlbums = new Paging<SavedAlbum>
            {
                Items = new List<SavedAlbum>
                {
                    new SavedAlbum
                    {
                        Album = new FullAlbum
                        {
                            Name = albumName,
                            Artists = new List<SimpleArtist>
                            {
                                new SimpleArtist
                                {
                                    Name = artistName
                                }
                            }
                        }
                    }
                }
            };

            Mocker.GetMock<ISpotifyProxy>().
                Setup(x => x.GetSavedAlbums(It.IsAny<SpotifySavedAlbums>(),
                                                It.IsAny<SpotifyClient>()))
                .Returns(savedAlbums);

            var result = Subject.Fetch(_api);

            result.Should().BeEmpty();
        }
    }
}
