﻿using System;
using System.Collections.Generic;
using ConcursModel.domain;
using ConcursPersistence.repository.Interface;
using log4net;

namespace ConcursServices
{
    public class ParticipantService
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ParticipantService));
        private readonly IParticipantRepository participantRepository;

        public ParticipantService(IParticipantRepository participantRepository)
        {
            log.Info("Creating ParticipantService");
            this.participantRepository = participantRepository;
        }

        public IEnumerable<Participant> FindAllParticipants()
        {
            log.Info("ParticipantService - FindAllParticipants");
            return participantRepository.FindAll();
        }

        public Participant FindParticipantById(int id)
        {
            log.Info($"ParticipantService - FindParticipantById with id: {id}");
            return participantRepository.FindOne(id);
        }

        public Participant SaveParticipant(Participant participant)
        {
            log.Info($"ParticipantService - SaveParticipant: {participant}");
            return participantRepository.Save(participant);
        }

        public Participant UpdateParticipant(Participant participant)
        {
            log.Info($"ParticipantService - UpdateParticipant: {participant}");
            return participantRepository.Update(participant);
        }

        public Participant DeleteParticipant(int id)
        {
            log.Info($"ParticipantService - DeleteParticipant with id: {id}");
            return participantRepository.Delete(id);
        }
    }
}