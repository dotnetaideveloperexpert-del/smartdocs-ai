using SmartDocs.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartDocs.Application.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
