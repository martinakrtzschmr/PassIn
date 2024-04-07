using PassIn.Communication.Requests;
using PassIn.Communication.Responses;
using PassIn.Exceptions;
using PassIn.Infrastructure;
using System.Net.Mail;

namespace PassIn.Application.UseCases.Events.RegisterAttendee;
public class RegisterAttendeeOnEventUseCase
{
    private readonly PassInDbContext _dbContext;
    public RegisterAttendeeOnEventUseCase()
    {
        _dbContext = new PassInDbContext();
    }

    public ResponseRegisteredJson Execute(Guid eventId, RequestRegisterEventJson request)
    {
        Validate(eventId, request);
        var entity = new Infrastructure.Entities.Attendee
        {
            Email = request.Email,
            Name = request.Name,
            Event_Id = eventId,
            Created_At = DateTime.UtcNow,
        };

        _dbContext.Attendees.Add(entity);
        _dbContext.SaveChanges();

        return new ResponseRegisteredJson
        {
            Id = entity.Id,
        };
    }

    private void Validate(Guid eventId, RequestRegisterEventJson request)
    {
        var eventEntity = _dbContext.Events.Find(eventId);
        if (eventEntity is null)
        {
            throw new NotFoundException("An event with this id doesn't exists.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ErrorOnValidationException("The name is invalid.");
        }

        if (!EmailIsValid(request.Email))
        {
            throw new ErrorOnValidationException("The email is invalid.");
        }

        var attendeeAlredyRegistered = _dbContext
            .Attendees
            .Any(attendee =>
                attendee.Email.Equals(request.Email)
                && attendee.Event_Id == eventId
            );
        if (attendeeAlredyRegistered)
        {
            throw new ConflictException("The user is alredy registered on this event.");
        }

        var attendeesForEvent = _dbContext.Attendees.Count(attendee => attendee.Event_Id == eventId);
        if (attendeesForEvent >= eventEntity.Maximum_Attendees)
        {
            throw new ErrorOnValidationException("All tickets for this event have been purchased.");
        }
    }

    private bool EmailIsValid(string email)
    {
        try
        {
            new MailAddress(email);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
