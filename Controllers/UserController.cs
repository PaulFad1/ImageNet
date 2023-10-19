namespace ImageNet.Controller
{
    
    using ImageNet.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Components.Forms;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UserController: ControllerBase
    {
        private AppContext db;
        private readonly IConfiguration configuration;
        public UserController(AppContext context, IConfiguration config)
        {
            db = context;
            configuration = config;

        }
        // POST user/register
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(string login, string password)
        {
            if(db.Users.Where(x => x.Login == login).FirstOrDefault() != null)
            {
                return BadRequest();
            }
            var user = new User() {Login = login, Password = password};
            db.Users.Add(user);
            await db.SaveChangesAsync();
            return Ok(user);
        }
        // GET user/images
        [HttpGet("images")]
        public IEnumerable<Image> Get()
        {
            string login = HttpContext.User.Identity.Name;
            var user = db.Users.Include(x => x.Images).Where(user => user.Login == login).First();
            return user.Images;
        }
        // POST user/uploadImage
        [HttpPost("uploadImage")]
        public async Task<ActionResult<Image>> Post(IFormFile imgFile)
        {
            var login = HttpContext.User.Identity.Name;
            string path = configuration.GetSection("PathToImageStorage").Value;
            if(!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filename = imgFile.FileName;
            string filenameWithPath = Path.Combine(path,filename);
            using(var stream = new FileStream(filenameWithPath, FileMode.Create))
            {
                imgFile.CopyTo(stream);
            }

            Image image = new();
            image.User = db.Users.Where(x => x.Login == login).First();
            image.Path = filename;
            db.Images.Add(image);
            await db.SaveChangesAsync();
            return Ok(image);
        }

        // DELETE user/deleteImage/{imgid}
        [HttpDelete("deleteImage/{imgid}")]
        public async Task<ActionResult> Delete(int imgid)
        {
            var login = HttpContext.User.Identity.Name;
            var users = db.Users.Include(x => x.Images);
            var user = users.Where(x => x.Login == login).First();
            var image = user.Images.Where(x => x.Id == imgid).FirstOrDefault();
            if(image == null)
            {
                return NotFound();
            }
            var path = configuration.GetSection("PathToImageStorage").Value;
            var pathToFile = Path.Combine(path, image.Path);
            FileInfo file = new FileInfo(pathToFile);
            if(file.Exists)
            {
                file.Delete();
            }
            db.Images.Remove(image);
            await db.SaveChangesAsync();
            return Ok();
        }

        // POST user/addFriend/{friendId}
        [HttpPost("addFriend/{friendId}")]
        public async Task<ActionResult> AddFriend(int friendId)
        {
            var login = HttpContext.User.Identity.Name;
            var friend = db.Users.Where(x => x.Id == friendId).FirstOrDefault();
            if(friend == null)
            {
                return NotFound();
            }
            var user = db.Users.Include(x => x.Friends).Where(x => x.Login == login).First();
            user.Friends.Add(friend);
            await db.SaveChangesAsync();
            return Ok();
        }

        //GET user/{id}/Images
        [HttpGet("{id}/Images")]
        public ActionResult<IEnumerable<Image>> GetImages(int id) // Отправляю объект Image
        {
            var login = HttpContext.User.Identity.Name;
            var users = db.Users.Include(x => x.Friends).Include(x => x.Images).ToList();
            if(users.Where(x => x.Id == id).FirstOrDefault() == null)
            {
                return NotFound();
            }
            var friend = users.Where(x => x.Id == id).FirstOrDefault();
            var user = users.Where(x => x.Login == login).FirstOrDefault();
            
            if(friend.Friends.Any(x => x.Id == user.Id))
            {
                return Ok(friend.Images);
            }
            else
            {
                return NoContent();
            }
        }
    }

}