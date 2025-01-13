using Microsoft.AspNetCore.Mvc;

namespace backend.Dtos.Teams;

public class TeamUpdate
{

    [FromRoute(Name = "id")]
    public string Id { get; set; }

    [FromBody]
    public TeamUpdateDetails Team { get; set; }
}
