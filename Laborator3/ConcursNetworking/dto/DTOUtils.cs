using System;
using ConcursModel.domain;
using ConcursNetworking.dto;

namespace ConcursNetworking.utils
{
    public class DTOUtils
    {
        public static User GetFromDTO(UserDTO userDto)
        {
            User user = new User(userDto.Username, userDto.Password, userDto.OfficeName)
            {
                Id = userDto.Id
            };
            return user;
        }

        public static UserDTO GetDTO(User user)
        {
            return new UserDTO
            {
                Id = user.Id,
                Username = user.Username,
                Password = user.Password,
                OfficeName = user.OfficeName
            };
        }

        public static Event GetFromDTO(EventDTO eventDto)
        {
            Event eventObj = new Event(eventDto.Distance, eventDto.Style)
            {
                Id = eventDto.Id
            };
            return eventObj;
        }

        public static EventDTO GetDTO(Event eventObj, int participantsCount)
        {
            return new EventDTO
            {
                Id = eventObj.Id,
                Distance = eventObj.Distance,
                Style = eventObj.Style,
                ParticipantsCount = participantsCount
            };
        }

        public static Participant GetFromDTO(ParticipantDTO participantDto)
        {
            Participant participant = new Participant(participantDto.Name, participantDto.Age)
            {
                Id = participantDto.Id
            };
            return participant;
        }

        public static ParticipantDTO GetDTO(Participant participant)
        {
            return new ParticipantDTO
            {
                Id = participant.Id,
                Name = participant.Name,
                Age = participant.Age
            };
        }
    }
}