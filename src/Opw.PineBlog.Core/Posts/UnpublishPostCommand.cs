using MediatR;
using Microsoft.EntityFrameworkCore;
using Opw.HttpExceptions;
using Opw.PineBlog.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Opw.PineBlog.Posts
{
    /// <summary>
    /// Command that unpublishes a post.
    /// </summary>
    public class UnpublishPostCommand : IRequest<Result<Post>>
    {
        /// <summary>
        /// The post id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Handler for the UnpublishPostCommand.
        /// </summary>
        public class Handler : IRequestHandler<UnpublishPostCommand, Result<Post>>
        {
            private readonly IBlogEntityDbContext _context;

            /// <summary>
            /// Implementation of UnpublishPostCommand.Handler.
            /// </summary>
            /// <param name="context">The blog entity context.</param>
            public Handler(IBlogEntityDbContext context)
            {
                _context = context;
            }

            /// <summary>
            /// Handle the UnpublishPostCommand request.
            /// </summary>
            /// <param name="request">The UnpublishPostCommand request.</param>
            /// <param name="cancellationToken">A cancellation token.</param>
            public async Task<Result<Post>> Handle(UnpublishPostCommand request, CancellationToken cancellationToken)
            {
                var entity = await _context.Posts.SingleOrDefaultAsync(e => e.Id.Equals(request.Id));
                if (entity == null)
                    return Result<Post>.Fail(new NotFoundException<Post>($"Could not find post, id: \"{request.Id}\""));

                entity.Published = null;
                
                _context.Posts.Update(entity);
                var result = await _context.SaveChangesAsync(true, cancellationToken);
                if (!result.IsSuccess)
                    return Result<Post>.Fail(result.Exception);

                return Result<Post>.Success(entity);
            }
        }
    }
}
