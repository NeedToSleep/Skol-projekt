﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Project_Booking.Model;

namespace Project_Booking
{
    public class BookingModel : PageModel
    {
        private readonly ConnectionContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookingModel(
            ConnectionContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public Booking CurrentBooking { get; set; }
        public ApplicationUser CurrentUser { get; set; }
        public Hotel CurrentHotel { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task OnGetAsync(string id)
        {
            CurrentHotel = await _context.Hotel.Where(h => h.Id == id).FirstOrDefaultAsync();

            var user = await _userManager.GetUserAsync(User);
            var userName = await _userManager.GetUserNameAsync(user);
            var nameOfUser = user.Name;
            var lastNameOfUser = user.LastName;

            CurrentUser = new ApplicationUser
            {
                UserName = userName,
                Name = nameOfUser,
                LastName = lastNameOfUser
            };
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            var user = await _userManager.GetUserAsync(User);
            CurrentUser = user;
            CurrentHotel = await _context.Hotel.Where(h => h.Id == id).FirstOrDefaultAsync();
            
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var book = new Booking()
            {
                ID = new Guid(),
                numOfBookedRooms = CurrentBooking.numOfBookedRooms,
                Customer = user,
                HotelID = CurrentHotel.Id,
                Hotel = CurrentHotel,
                Name = CurrentBooking.Name,
                LastName = CurrentBooking.LastName,
                CheckIn = CurrentBooking.CheckIn,
                CheckOut = CurrentBooking.CheckOut
            };
            user.MyBookings.Add(book);
            await _context.Booking.AddAsync(book);
            await _context.SaveChangesAsync();

            return RedirectToPage("Account/Manage/MyBookings", StatusMessage = "Booking has been added");
        }
    }
}