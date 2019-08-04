using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Opw.HttpExceptions;
using Opw.PineBlog.Entities;
using Opw.PineBlog.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Opw.PineBlog.Posts
{
    public class GetPostQuery : IRequest<Result<PostModel>>
    {
        public string Slug { get; set; }

        public class Handler : IRequestHandler<GetPostQuery, Result<PostModel>>
        {
            private readonly IOptions<PineBlogOptions> _blogOptions;
            private readonly IBlogEntityDbContext _context;

            public Handler(IBlogEntityDbContext context, IOptions<PineBlogOptions> blogOptions)
            {
                _blogOptions = blogOptions;
                _context = context;
            }

            public async Task<Result<PostModel>> Handle(GetPostQuery request, CancellationToken cancellationToken)
            {
                var post = await _context.Posts
                    .Include(p => p.Author)
                    .Where(p => p.Published != null)
                    .Where(p => p.Slug.Equals(request.Slug))
                    .SingleOrDefaultAsync(cancellationToken);

                if (post == null)
                    return Result<PostModel>.Fail(new NotFoundException<Post>($"Could not find post for slug: \"{request.Slug}\""));

                var model = new PostModel
                {
                    Blog = new BlogModel(_blogOptions.Value),
                    Post = post,
                    Next = null,
                    Previous = null
                };

                if (string.IsNullOrWhiteSpace(model.Post.CoverUrl))
                {
                    model.Post.CoverUrl = model.Blog.CoverUrl;
                    model.Post.CoverCaption = model.Blog.CoverCaption;
                    model.Post.CoverLink = model.Blog.CoverLink;
                }

                model.Next = await _context.Posts
                    .Where(p => p.Published > post.Published)
                    .OrderBy(p => p.Published)
                    .Take(1)
                    .SingleOrDefaultAsync(cancellationToken);

                model.Previous = await _context.Posts
                    .Where(p => p.Published < post.Published)
                    .OrderByDescending(p => p.Published)
                    .Take(1)
                    .SingleOrDefaultAsync(cancellationToken);

                return Result<PostModel>.Success(model);
            }
        }
    }
}
