using Microsoft.EntityFrameworkCore;
using PassIn.Communication.Responses;
using PassIn.Exceptions;
using PassIn.Infrastructure;

namespace PassIn.Application.UseCases.Attendees.GetAllByEventsId;
public class GetAllAttendeesByEventsIdUseCase
{
    private readonly PassInDbContext _dbcontext;
    public GetAllAttendeesByEventsIdUseCase()
    {
        _dbcontext = new PassInDbContext();
    }
    public ResponseAllAttendeesJson Execute(Guid eventId)
    {
        var entity = _dbcontext.Events.Include(ev => ev.Attendees).ThenInclude(attendee => attendee.CheckIn).FirstOrDefault(ev => ev.Id == eventId);
        if (entity is null)
        {
            throw new NotFoundException("An event with this id doesn't exists.");
        }

        return new ResponseAllAttendeesJson
        {
            Attendees = entity.Attendees.Select(attendee => new ResponseAttendeeJson
            {
                Id = attendee.Id,
                Name = attendee.Name,
                Email = attendee.Email,
                CreatedAt = attendee.Created_At,
                CheckedInAt = attendee.CheckIn?.Created_At
            }).ToList()
        };
    }
}
