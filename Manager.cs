using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace winui_db
{
    class Manager
    {
        public static async Task<User> GetUserFromDatabaseAsync(string email, string password)
        {
            using (var database = new Database())
            {
                var user = await database.Users.FirstOrDefaultAsync(i => i.Email == email);
                if (user != null && user.Password != password)
                {
                    user = null; // Неправильный пароль
                }
                return user;
            }
        }

        public static async Task<Medicine> GetByIdFromDatabaseAsync(int id)
        {
            using (var database = new Database())
            {
                return await database.Catalog.FirstOrDefaultAsync(i => i.Id == id);
            }
        }

        public static async Task<List<Medicine>> GetByCategoryFromDatabaseAsync(string category)
        {
            using (var database = new Database())
            {
                return await database.Catalog.Where(i => i.Category == category).ToListAsync();
            }
        }

        public async static Task<bool> TryAddToCartAsync(int id, string key)
        {
            try
            {
                using (var database = new Database())
                {
                   
                        // Получаем пользователя
                        var user = await database.Users.FirstOrDefaultAsync(u => u.Email == key);

                        if (user == null)
                            return false;

                        
                        // Пытаемся получить товар из базы данных
                        var medicine = await GetByIdFromDatabaseAsync(id);
                        if (medicine == null)
                            return false;

                        // Ищем товар в корзине пользователя
                        var existingItem = await database.CartViews.Where(sc => sc.CartId == user.Id && sc.Id == id).FirstOrDefaultAsync();
                        if (existingItem != null)
                        {
                            // Увеличиваем количество товара, если он уже есть в корзине
                            existingItem.Count += 1;
                        }
                        else
                        {
                            // Добавляем товар в корзину, если его там нет
                            database.CartViews.Add(new MedicineShoppingCartView() { Id = medicine.Id, Count = 1, CartId = user.Id });
                           //database.Users.Add(new User(){ Email = "kkk", Name = "", Password = "kkk", Role = "Admin"});
                        }

                        // Сохраняем изменения в базе данных
                        await database.SaveChangesAsync();

                        
                       

                        return true; // Операция выполнена успешно
                    
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибок
                Console.WriteLine(ex);
                return false;
            }
        }

        public static async Task<bool> TryDeleteFromCartAsync(int id, User user)
        {
            using (var database = new Database())
            {
                // var cart = await database.ShoppingCarts.Include(sc => sc.View).Where(c => c.Id == user.Id).FirstOrDefaultAsync();
                // var itemToRemove = cart.View.FirstOrDefault(c => c.Id == id);
                // if (itemToRemove == null) return false;

                // cart.View.Remove(itemToRemove);

                // await database.SaveChangesAsync();
                return true;
            }
        }

        public static async Task<bool> TryEditCountAsync(int id, int count, User user)
        {
            using (var database = new Database())
            {
                // var cart = await database.ShoppingCarts.Include(sc => sc.View).Where(c => c.Id == user.Id).FirstOrDefaultAsync();
                // var item = cart.View.FirstOrDefault(m => m.Id == id);
                // if (item == null) return false;

                // item.Count = count;

                // await database.SaveChangesAsync();
                return true;
            }
        }

        public static async Task<bool> TryAddUserAsync(string name, string email, string password)
        {
            using (var database = new Database())
            {
                try
                {
                    var existingUser = await database.Users.FirstOrDefaultAsync(u => u.Email == email);
                    if (existingUser != null)
                    {
                        Console.WriteLine("User with this email already exists.");
                        return false;
                    }

                    var user = new User() { Email = email, Name = name, Password = password, Role = "Customer" };
                    database.Users.Add(user);
                    await database.SaveChangesAsync();
                    Console.WriteLine("SUCCESS SIGN UP");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return false;
                }
                return true;
            }
        }

        public static List<(string Category, string Name, string ActiveComponent, string Description, string ReleaseForm, Countries Country, int Expiration, double Price)> Init()
        {
            return new List<(string Category, string Name, string ActiveComponent, string Description, string ReleaseForm, Countries Country, int Expiration, double Price)>
            {
                //fever
                ("Fever Medicine", "NyQuil", "Acetaminophen, Dextromethorphan, Doxylamine", "NyQuil relieves cold symptoms like cough, congestion, and sore throat, providing restful sleep. It's available in liquid form and is a trusted choice for nighttime relief.", "Liquid capsules", Countries.USA, 2, 9.99),
                ("Fever Medicine", "Sudafed", "Pseudoephedrine", "Sudafed is a decongestant that clears nasal congestion due to colds, allergies, or sinus infections. It comes in tablet form and provides fast relief from stuffiness.", "Tablets", Countries.UK, 3, 5.49),
                ("Fever Medicine", "Benadryl", "Diphenhydramine", "Benadryl offers relief from cold symptoms like runny nose, sneezing, and itching. Its antihistamine properties provide effective allergy relief in tablet form, allowing for better sleep.", "Tablets", Countries.USA, 2, 7.99),
                ("Fever Medicine", "Mucinex", "Guaifenesin", "Mucinex thins and loosens mucus to relieve chest congestion caused by colds and chest infections. Its extended-release tablets provide long-lasting relief from cough and phlegm buildup.", "Extended-release tablets", Countries.USA, 2, 12.49),
                ("Fever Medicine", "DayQuil", "Acetaminophen, Dextromethorphan, Phenylephrine", "DayQuil offers non-drowsy relief from cold symptoms like headache, fever, and nasal congestion. Its multi-symptom formula in capsule form provides all-day comfort and productivity.", "Capsules", Countries.USA, 2, 8.99),
                ("Fever Medicine", "Robitussin", "Dextromethorphan", "Robitussin suppresses coughs and relieves chest congestion caused by colds and flu. Its syrup form soothes throat irritation and helps loosen mucus for easier breathing.", "Syrup", Countries.USA, 3, 10.99),
                
                //ent
                ("ENT Medicine", "Flonase", "Fluticasone propionate", "Flonase is a nasal spray that relieves symptoms of allergic rhinitis, including nasal congestion, sneezing, and runny nose. Its corticosteroid formula reduces inflammation in the nasal passages for long-lasting relief.", "Nasal spray", Countries.USA, 1, 14.99),
                ("ENT Medicine", "Claritin", "Loratadine", "Claritin provides non-drowsy relief from seasonal and perennial allergies, such as hay fever and pet allergies. Its antihistamine tablets alleviate symptoms like itchy eyes, sneezing, and nasal congestion.", "Tablets", Countries.USA, 2, 11.49),
                ("ENT Medicine", "Afrin", "Oxymetazoline", "Afrin nasal spray quickly relieves nasal congestion caused by colds, allergies, or sinusitis. Its vasoconstrictor action shrinks swollen nasal tissues, allowing for easier breathing within seconds.", "Nasal spray", Countries.USA, 1, 9.99),
                ("ENT Medicine", "Tylenol Sinus", "Acetaminophen, Phenylephrine", "Tylenol Sinus provides relief from sinus headaches, pain, and congestion associated with colds or sinus infections. Its combination formula in caplets targets multiple symptoms for effective relief.", "Caplets", Countries.USA, 2, 8.99),
                ("ENT Medicine", "Nasonex", "Mometasone furoate", "Nasonex nasal spray treats nasal symptoms of allergic rhinitis, such as congestion, sneezing, and itching. Its corticosteroid formula reduces inflammation in the nasal passages, providing long-lasting relief.", "Nasal spray", Countries.USA, 1, 17.99),
                ("ENT Medicine", "Zyrtec-D", "Cetirizine, Pseudoephedrine", "Zyrtec-D offers relief from allergy symptoms and nasal congestion due to colds or allergies. Its combination formula in extended-release tablets provides non-drowsy relief for up to 12 hours.", "Extended-release tablets", Countries.USA, 2, 13.49),
                ("ENT Medicine", "Vicks VapoRub", "Camphor, Menthol, Eucalyptus oil", "Vicks VapoRub is a topical ointment that provides relief from coughs and nasal congestion. Its medicated vapors are inhaled to soothe chest and throat discomfort, promoting easier breathing and a good night's sleep.", "Ointment", Countries.USA, 3, 6.99),

                //ophtalmology
                ("Ophthalmology Medicine", "Visine", "Tetrahydrozoline hydrochloride", "Visine eye drops relieve redness, itching, and irritation caused by allergies, dryness, or minor eye irritants. Its fast-acting formula provides quick relief and soothes eyes for up to 12 hours.", "Eye drops", Countries.USA, 1, 6.99),
                ("Ophthalmology Medicine", "Refresh Tears", "Carboxymethylcellulose sodium", "Refresh Tears lubricating eye drops provide long-lasting relief from dry, irritated eyes. Its gentle formula mimics natural tears, hydrating and soothing the eyes for instant comfort and improved vision.", "Eye drops", Countries.USA, 2, 8.49),
                ("Ophthalmology Medicine", "Alphagan", "Brimonidine tartrate", "Alphagan eye drops lower intraocular pressure (IOP) in patients with glaucoma or ocular hypertension. Its alpha-2 adrenergic agonist action reduces fluid production in the eye, preventing optic nerve damage and vision loss.", "Eye drops", Countries.USA, 1, 15.99),
                ("Ophthalmology Medicine", "Amogus Ultra", "Imposters", "The imposter is sus!", "Eye drops", Countries.USA, 1, 10.99),
                ("Ophthalmology Medicine", "Bebra Ultra", "Morgenshtern", "Vieb goroda zarabotal deneg nyne moyu ahopy katit novy merin", "Vitamins", Countries.USA, 1, 12.99),
                ("Ophthalmology Medicine", "Pure Maui", "Poison", "The purest essense of fucking development.", "Eye drops", Countries.USA, 1, 13.99),
                
                //gastroenterology
                ("Gastroenterology Medicine", "Prilosec", "Omeprazole", "Prilosec is a proton pump inhibitor (PPI) that reduces stomach acid production, providing relief from heartburn, acid reflux, and ulcers. Its delayed-release capsules offer long-lasting protection for up to 24 hours.", "Delayed-release capsules", Countries.USA, 2, 15.99),
                ("Gastroenterology Medicine", "Imodium", "Loperamide", "Imodium controls diarrhea by slowing down bowel movements and reducing stool frequency. Its caplets provide quick relief from acute diarrhea and traveler's diarrhea, allowing for better hydration and comfort.", "Caplets", Countries.USA, 3, 9.49),
                ("Gastroenterology Medicine", "Nexium", "Esomeprazole", "Nexium is a PPI that treats gastroesophageal reflux disease (GERD), erosive esophagitis, and ulcers. Its delayed-release capsules provide long-lasting relief from acid-related conditions, promoting healing and symptom control.", "Delayed-release capsules", Countries.USA, 2, 17.99),
                ("Gastroenterology Medicine", "Pepto-Bismol", "Bismuth subsalicylate", "Pepto-Bismol relieves symptoms of indigestion, upset stomach, and nausea caused by overeating or stomach flu. Its chewable tablets provide a soothing coating action and reduce diarrhea frequency.", "Chewable tablets", Countries.USA, 2, 7.99),
                ("Gastroenterology Medicine", "Miralax", "Polyethylene glycol 3350", "Miralax is an osmotic laxative that relieves constipation by drawing water into the colon, softening stool and promoting regular bowel movements. Its powder form dissolves easily in water or juice for easy ingestion.", "Powder for oral solution", Countries.USA, 2, 10.99),
                ("Gastroenterology Medicine", "Gas-X", "Simethicone", "Gas-X relieves gas, bloating, and discomfort caused by gas accumulation in the digestive tract. Its chewable tablets break down gas bubbles for quick relief, allowing for easier digestion and comfort after meals.", "Chewable tablets", Countries.USA, 2, 8.49),
                ("Gastroenterology Medicine", "Colace", "Docusate sodium", "Colace is a stool softener that relieves constipation by adding moisture to stool, making it easier to pass. Its capsules provide gentle and effective relief from occasional constipation without causing cramping or harsh side effects.", "Capsules", Countries.USA, 3, 9.99),
                ("Gastroenterology Medicine", "Reglan", "Metoclopramide", "Reglan helps relieve symptoms of gastrointestinal disorders like gastroparesis and acid reflux by increasing stomach contractions and emptying. Its oral tablets provide fast relief from nausea, vomiting, and heartburn.", "Tablets", Countries.USA, 2, 12.99),
                ("Gastroenterology Medicine", "Zantac", "Ranitidine", "Zantac is an H2 blocker that reduces stomach acid production, providing relief from heartburn, acid indigestion, and sour stomach. Its tablets offer fast-acting relief and prevention of acid-related symptoms for up to 12 hours.", "Tablets", Countries.USA, 2, 14.49),


                //bones
                ("Bone Health Medicine", "Fosamax", "Alendronate", "Fosamax is a bisphosphonate medication used to treat osteoporosis and prevent bone fractures. It works by slowing down bone loss and increasing bone density, reducing the risk of fractures.", "Tablets", Countries.USA, 3, 20.99),
                ("Bone Health Medicine", "Boniva", "Ibandronate", "Boniva is a bisphosphonate medication prescribed for osteoporosis treatment and prevention. It helps increase bone density and reduce the risk of fractures in postmenopausal women.", "Tablets", Countries.USA, 2, 25.99),
                ("Bone Health Medicine", "Calcitonin", "Calcitonin salmon", "Calcitonin is a hormone medication used to treat osteoporosis in postmenopausal women. It helps slow down bone loss and reduce the risk of fractures by inhibiting bone resorption.", "Nasal spray", Countries.USA, 2, 30.99),
                ("Bone Health Medicine", "Evista", "Raloxifene", "Evista is a selective estrogen receptor modulator (SERM) prescribed for osteoporosis treatment and prevention in postmenopausal women. It helps increase bone density and reduce the risk of fractures.", "Tablets", Countries.USA, 2, 22.99),
                ("Bone Health Medicine", "Prolia", "Denosumab", "Prolia is a monoclonal antibody medication used to treat osteoporosis in postmenopausal women and men at high risk of fractures. It helps increase bone density and reduce the risk of fractures by inhibiting bone resorption.", "Injection", Countries.USA, 1, 200.99),
                ("Bone Health Medicine", "Forteo", "Teriparatide", "Forteo is a synthetic form of parathyroid hormone used to treat osteoporosis in men and postmenopausal women at high risk of fractures. It helps stimulate new bone formation and increase bone density.", "Injection", Countries.USA, 1, 500.99),
                ("Bone Health Medicine", "Actonel", "Risedronate", "Actonel is a bisphosphonate medication used to treat osteoporosis and prevent bone fractures. It works by slowing down bone loss and increasing bone density, reducing the risk of fractures.", "Tablets", Countries.USA, 3, 18.99),
                ("Bone Health Medicine", "Reclast", "Zoledronic acid", "Reclast is a bisphosphonate medication used to treat osteoporosis and prevent bone fractures. It helps increase bone density and reduce the risk of fractures with a single annual infusion.", "Infusion", Countries.USA, 1, 1000.99),

                //veterinary
                ("Veterinary Medicine", "Rimadyl", "Carprofen", "Rimadyl is a non-steroidal anti-inflammatory drug (NSAID) prescribed for dogs to relieve pain and inflammation associated with osteoarthritis and postoperative pain. Its chewable tablets provide effective relief for improved mobility and comfort.", "Chewable tablets", Countries.USA, 2, 30.99),
                ("Veterinary Medicine", "Revolution", "Selamectin", "Revolution is a topical medication for cats and dogs that provides broad-spectrum protection against fleas, ticks, heartworms, ear mites, and intestinal parasites. Its monthly application is easy to administer and offers comprehensive parasite control.", "Topical solution", Countries.USA, 1, 40.99),
                ("Veterinary Medicine", "Baytril", "Enrofloxacin", "Baytril is an antibiotic medication prescribed for dogs and cats to treat bacterial infections of the skin, urinary tract, and respiratory system. Its oral tablets provide effective treatment for a wide range of bacterial diseases.", "Oral tablets", Countries.USA, 2, 25.99),
                ("Veterinary Medicine", "Dogsy Treats", "Meat Cuts", "The imposter is sus!", "Pills", Countries.USA, 1, 3.99),
                ("Veterinary Medicine", "Cat Mint", "Mint", "Let's het high!", "Pills", Countries.USA, 1, 2.99),
                ("Veterinary Medicine", "Bark-Bark", "Vitamin D", "Stay healty little doggie!", "Tablets", Countries.USA, 1, 4.99),

                //Dental Medicine
                ("Dental Medicine", "Sensodyne", "Potassium nitrate, Sodium fluoride", "Sensodyne toothpaste is specially formulated to relieve tooth sensitivity and protect against cavities. Its fluoride-rich formula strengthens enamel and soothes sensitive teeth for long-lasting relief and improved oral health.", "Toothpaste", Countries.USA, 2, 5.99),
                ("Dental Medicine", "Crest Pro-Health", "Stannous fluoride", "Crest Pro-Health toothpaste provides comprehensive oral care by fighting cavities, plaque, gingivitis, and sensitivity. Its advanced formula strengthens enamel and freshens breath for a healthier mouth.", "Toothpaste", Countries.USA, 2, 4.99),
                ("Dental Medicine", "Oral-B Glide", "PTFE (polytetrafluoroethylene)", "Oral-B Glide dental floss is designed to effectively remove plaque and food particles from between teeth and below the gumline. Its smooth texture slides easily and comfortably, promoting better oral hygiene and gum health.", "Dental floss", Countries.USA, 2, 3.49),
                ("Dental Medicine", "Listerine", "Eucalyptol, Menthol, Thymol", "Listerine mouthwash kills germs that cause bad breath, plaque, and gum disease. Its antiseptic formula freshens breath and provides 24-hour protection against oral bacteria for a healthier mouth.", "Mouthwash", Countries.USA, 2, 6.99),
                ("Dental Medicine", "ACT", "Fluoride", "ACT mouthwash strengthens enamel and prevents cavities by delivering fluoride to teeth. Its alcohol-free formula freshens breath and provides long-lasting protection against tooth decay and acid erosion.", "Mouthwash", Countries.USA, 2, 7.99),
                ("Dental Medicine", "Colgate Total", "Triclosan, Sodium fluoride", "Colgate Total toothpaste fights bacteria, plaque, and gingivitis while providing all-around protection for teeth and gums. Its fluoride-rich formula strengthens enamel and freshens breath for a healthier smile.", "Toothpaste", Countries.USA, 2, 4.99),
                ("Dental Medicine", "Waterpik", "N/A (Oral irrigation)", "Waterpik water flosser uses pulsating water to clean between teeth and below the gumline, removing plaque and debris for healthier gums and teeth. Its adjustable pressure settings and multiple tips make it suitable for all dental needs.", "Oral irrigation device", Countries.USA, 3, 59.99),
                ("Dental Medicine", "Sensodyne Rapid Relief", "Potassium nitrate, Sodium fluoride", "Sensodyne Rapid Relief toothpaste provides fast relief from tooth sensitivity pain and strengthens enamel against future sensitivity. Its clinically proven formula offers long-lasting protection for sensitive teeth.", "Toothpaste", Countries.USA, 2, 6.99),
                ("Dental Medicine", "Tom's of Maine", "Sodium fluoride", "Tom's of Maine toothpaste offers natural oral care solutions made with fluoride and naturally derived ingredients. Its fluoride-rich formula fights cavities, freshens breath, and promotes overall dental health without artificial flavors or colors.", "Toothpaste", Countries.USA, 2, 4.99),
                ("Dental Medicine", "Oral-B Pro-Health", "Stannous fluoride", "Oral-B Pro-Health toothpaste protects against cavities, plaque, gingivitis, sensitivity, and bad breath. Its advanced formula strengthens enamel and fights bacteria for comprehensive oral care and improved dental health.", "Toothpaste", Countries.USA, 2, 15.99),

                //Woman Health Accesoires
                
                ("Woman Health Accesoires", "Always Ultra Thin Pads", "N/A", "Always Ultra Thin Pads offer reliable protection and comfort during menstruation. Their flexible design with wings provides maximum absorbency and leakage protection for all-day confidence.", "Menstrual pads", Countries.USA, 0, 6.99),
                ("Woman Health Accesoires", "Tampax Pearl Tampons", "N/A", "Tampax Pearl Tampons feature a smooth plastic applicator and a LeakGuard braid for secure protection against leaks. Their compact size and discreet packaging make them convenient for on-the-go use.", "Menstrual tampons", Countries.USA, 0, 7.99),
                ("Woman Health Accesoires", "DivaCup Menstrual Cup", "N/A", "DivaCup Menstrual Cup offers eco-friendly and cost-effective menstrual protection. Made of medical-grade silicone, it provides leak-free protection for up to 12 hours and can be reused for years.", "Menstrual cup", Countries.USA, 0, 29.99),
                ("Woman Health Accesoires", "Summer's Eve Cleansing Wash", "N/A", "Summer's Eve Cleansing Wash is pH-balanced and specially formulated for feminine hygiene. Its gentle formula cleanses and refreshes without irritation, leaving you feeling clean and confident.", "Feminine wash", Countries.USA, 0, 4.99),
                ("Woman Health Accesoires", "Vagisil Odor Block Intimate Powder", "N/A", "Vagisil Odor Block Intimate Powder absorbs moisture and neutralizes odor for long-lasting freshness. Its talc-free formula is gentle on sensitive skin and helps prevent chafing and discomfort.", "Intimate powder", Countries.USA, 0, 5.49),
                ("Woman Health Accesoires", "Always Discreet Incontinence Liners", "N/A", "Always Discreet Incontinence Liners offer discreet bladder protection for women experiencing light bladder leaks. Their thin design and absorbent core provide comfortable and reliable protection.", "Incontinence liners" ,Countries.USA, 0, 8.99),
                ("Woman Health Accesoires", "L. Organic Cotton Pads", "N/A", "L. Organic Cotton Pads are made with organic cotton and free from harmful chemicals. Their breathable design and absorbent core provide comfortable and safe menstrual protection.", "Menstrual pads", Countries.USA, 0, 7.49),
                ("Woman Health Accesoires", "Summer's Eve Deodorant Spray", "N/A", "Summer's Eve Deodorant Spray keeps you feeling fresh and confident all day long. Its gentle formula neutralizes odor and provides a light, refreshing scent for intimate freshness.", "Deodorant spray", Countries.USA, 0, 3.99),
                ("Woman Health Accesoires", "Always Infinity FlexFoam Pads", "N/A", "Always Infinity FlexFoam Pads feature a unique FlexFoam core that absorbs 10 times its weight. Their super thin design and form-fitting shape provide comfortable and reliable protection for heavy flow days.", "Menstrual pads", Countries.USA, 0, 9.99),
                ("Woman Health Accesoires", "L. Organic Cotton Tampons","N/A", "L. Organic Cotton Tampons are made with organic cotton and free from synthetic materials. Their smooth applicator and leak-resistant design provide comfortable and reliable protection during menstruation.", "Menstrual tampons", Countries.USA, 0, 8.49),
                ("Woman Health Accesoires", "Kotex U Click Compact Tampons","N/A", "Kotex U Click Compact Tampons feature a small, discreet design and a smooth applicator for easy insertion. Their compact size makes them convenient to carry in your purse or pocket.", "Menstrual tampons", Countries.USA, 0, 6.99),
                ("Woman Health Accesoires", "Poise Incontinence Pads", "N/A","Poise Incontinence Pads offer discreet protection for women experiencing light bladder leakage. Their absorb-loc core and leak-block sides provide dryness and odor control for all-day confidence.", "Incontinence pads", Countries.USA, 0, 10.99),
                ("Woman Health Accesoires", "Always Radiant FlexFoam Pads","N/A", "Always Radiant FlexFoam Pads feature a unique FlexFoam core that moves with you for comfortable protection. Their light, clean scent and soft, breathable top layer provide a fresh feeling all day long.", "Menstrual pads", Countries.USA, 0, 7.99),

                //Child Care Accesoires

		        ("Child Care Accessories", "Pampers Swaddlers Diapers", "N/A", "Pampers Swaddlers Diapers offer softness and protection for newborns. Their absorb-away liner pulls wetness and mess away from baby's skin for up to 12 hours of protection, while the umbilical cord notch keeps the area dry and comfortable.", "Disposable diapers", Countries.USA, 0, 24.99),
                ("Child Care Accessories", "Johnson's Baby Shampoo", "N/A", "Johnson's Baby Shampoo is gentle on baby's delicate skin and eyes. Its tear-free formula cleanses without drying, leaving hair soft, shiny, and smelling fresh.", "Baby shampoo", Countries.USA, 0, 3.99),
                ("Child Care Accessories", "Huggies Natural Care Wipes", "N/A", "Huggies Natural Care Wipes are made with 99% water and plant-based materials, making them gentle and safe for baby's sensitive skin. Their thick and soft texture provides effective cleaning and moisturizing.", "Baby wipes", Countries.USA, 0, 5.49),
                ("Child Care Accessories", "Desitin Maximum Strength Diaper Rash Cream", "N/A", "Desitin Maximum Strength Diaper Rash Cream provides relief and protection against diaper rash. Its zinc oxide formula creates a protective barrier to soothe and heal irritated skin.", "Diaper rash cream", Countries.USA, 0, 6.99),
                ("Child Care Accessories", "Aveeno Baby Daily Moisture Lotion", "N/A", "Aveeno Baby Daily Moisture Lotion nourishes and protects baby's delicate skin. Its non-greasy formula with natural colloidal oatmeal moisturizes for 24 hours, leaving skin soft and smooth.", "Baby lotion", Countries.USA, 0, 7.99),
                ("Child Care Accessories", "Gerber Graduates Puffs", "N/A", "Gerber Graduates Puffs are melt-in-your-mouth snacks for babies learning to self-feed. Their bite-sized pieces are easy to grasp and dissolve quickly, making them perfect for little ones exploring new tastes and textures.", "Baby snacks", Countries.USA, 0, 3.49),
                ("Child Care Accessories", "Aquaphor Baby Healing Ointment", "N/A", "Aquaphor Baby Healing Ointment soothes and protects dry, chapped, or irritated skin. Its gentle formula is fragrance-free and hypoallergenic, making it suitable for sensitive skin and diaper rash prevention.", "Healing ointment", Countries.USA, 0, 8.99),
                ("Child Care Accessories", "Dreft Stage 1: Newborn Liquid Laundry Detergent", "N/A", "Dreft Stage 1: Newborn Liquid Laundry Detergent is specially formulated for baby's delicate skin. Its hypoallergenic formula removes stains and odors without harsh chemicals, leaving clothes clean and soft.", "Laundry detergent", Countries.USA, 0, 12.99),
                ("Child Care Accessories", "Babyganics Alcohol-Free Foaming Hand Sanitizer", "N/A", "Babyganics Alcohol-Free Foaming Hand Sanitizer kills 99.9% of germs without drying baby's hands. Its gentle, plant-based formula is fragrance-free and safe for frequent use.", "Hand sanitizer", Countries.USA, 0, 4.99),
                ("Child Care Accessories", "NUK Simply Natural Baby Bottle", "N/A", "NUK Simply Natural Baby Bottle mimics the shape and feel of mom's breast for comfortable feeding. Its soft silicone nipple with multiple feeding holes provides a natural flow and reduces colic.", "Baby bottle", Countries.USA, 0, 6.99),
                ("Child Care Accessories", "Boogie Wipes Saline Nose Wipes", "N/A", "Boogie Wipes Saline Nose Wipes gently clean and moisturize baby's nose, making it easier to breathe. Their hypoallergenic formula with saline helps dissolve mucus and soothe dry, irritated skin.", "Saline nose wipes", Countries.USA, 0, 3.99),
                ("Child Care Accessories", "Baby Mum-Mum Rice Rusks", "N/A", "Baby Mum-Mum Rice Rusks are gluten-free teething biscuits for babies. Their dissolvable texture and gentle flavor make them ideal for soothing teething pain and promoting oral development.", "Teething biscuits", Countries.USA, 0, 2.99),



                //SPA
		        ("SPA", "Epsom Salt", "N/A", "Epsom Salt is known for its therapeutic properties, promoting relaxation and muscle recovery. Add it to warm bathwater to soothe sore muscles, relieve stress, and detoxify the body.", "Bath salt", Countries.USA, 0, 4.99),
                ("SPA", "Lavender Essential Oil", "N/A", "Lavender Essential Oil is renowned for its calming and aromatherapeutic properties. Add a few drops to a diffuser or bathwater to promote relaxation, reduce stress, and improve sleep quality.", "Essential oil", Countries.USA, 0, 8.99),
                ("SPA", "Facial Sheet Masks", "N/A", "Facial Sheet Masks are infused with hydrating and nourishing ingredients to rejuvenate the skin. Apply the mask to clean skin and relax for 15-20 minutes to enjoy a spa-like experience at home.", "Sheet mask", Countries.USA, 0, 3.49),
                ("SPA", "Foot Spa Bath Massager", "N/A", "Foot Spa Bath Massager provides a luxurious foot massage experience at home. Its bubbling and vibrating functions soothe tired feet, relieve muscle tension, and improve circulation.", "Foot spa", Countries.USA, 0, 29.99),
                ("SPA", "Aromatherapy Candles", "N/A", "Aromatherapy Candles are infused with essential oils to create a relaxing ambiance and promote well-being. Light the candle to fill the room with soothing scents and unwind after a long day.", "Candle", Countries.USA, 0, 12.99),
                ("SPA", "Almond Essential Oil", "N/A", "Almond Essential Oil is renowned for its calming and aromatherapeutic properties. Add a few drops to a diffuser or bathwater to promote relaxation, reduce stress, and improve sleep quality.", "Essential oil", Countries.USA, 0, 8.99),

                //Contraception
		        ("Contraception", "Condoms", "N/A", "Condoms are a barrier method of contraception that helps prevent pregnancy and reduce the risk of sexually transmitted infections (STIs). Available in various sizes, textures, and materials, condoms provide reliable protection during sexual intercourse.", "Condom", Countries.USA, 0, 9.99),
                ("Contraception", "Birth Control Pills", "N/A", "Birth Control Pills are oral contraceptives that contain hormones to prevent ovulation and thicken cervical mucus, making it difficult for sperm to reach the egg. Taken daily, these pills offer effective pregnancy prevention when used correctly.", "Oral contraceptive", Countries.USA, 0, 15.99),
                ("Contraception", "Emergency Contraceptive Pill (Morning-After Pill)", "N/A", "Emergency Contraceptive Pill, also known as the Morning-After Pill, is a backup method of contraception used to prevent pregnancy after unprotected sex or contraceptive failure. It works by delaying ovulation or preventing fertilization.", "Oral contraceptive", Countries.USA, 0, 49.99),
                ("Contraception", "Birth Control Patch", "N/A", "Birth Control Patch is a transdermal contraceptive patch that delivers hormones through the skin to prevent pregnancy. Applied weekly, it works by suppressing ovulation and thickening cervical mucus to inhibit sperm penetration.", "Transdermal patch", Countries.USA, 0, 29.99),
                ("Contraception", "Contraceptive Sponge", "N/A", "Contraceptive Sponge is a soft, disposable foam device inserted into the vagina to prevent pregnancy. It contains spermicide and covers the cervix to block sperm from entering the uterus. Effective for up to 24 hours, it provides convenient and discreet contraception.", "Vaginal contraceptive", Countries.USA, 0, 12.99),
                ("Contraception", "Intrauterine Device (IUD)", "N/A", "Intrauterine Device (IUD) is a long-acting reversible contraceptive inserted into the uterus by a healthcare provider. Available in hormonal and non-hormonal options, it provides highly effective, low-maintenance contraception for several years.", "Intrauterine device", Countries.USA, 0, 300), // Price range specified in description
    		    ("Contraception", "Diaphragm", "N/A", "Diaphragm is a barrier method of contraception that covers the cervix and prevents sperm from reaching the uterus. Used with spermicide, it offers reliable pregnancy prevention and can be inserted discreetly before intercourse.", "Vaginal contraceptive", Countries.USA, 0, 49.99),



                //First Aid Medicine
		        ("First Aid Medicine", "Band-Aid Flexible Fabric Bandages", "N/A", "Band-Aid Flexible Fabric Bandages offer durable protection for minor cuts and scrapes. Their flexible fabric stretches and flexes with movement for lasting comfort and protection.", "Adhesive bandages", Countries.USA, 0, 3.99),
                ("First Aid Medicine", "Neosporin First Aid Antibiotic Ointment", "N/A", "Neosporin First Aid Antibiotic Ointment prevents infection and promotes wound healing. Its triple antibiotic formula kills bacteria and provides long-lasting protection against infection.", "Antibiotic ointment", Countries.USA, 0, 6.99),
                ("First Aid Medicine", "ACE Elastic Bandage", "N/A", "ACE Elastic Bandage provides compression and support for sprains, strains, and other injuries. Its breathable material and adjustable design ensure a comfortable fit for all-day wear.", "Elastic bandage", Countries.USA, 0, 7.49),
                ("First Aid Medicine", "Benadryl Itch Relief Stick", "N/A", "Benadryl Itch Relief Stick provides fast relief from insect bites, poison ivy, oak, and sumac. Its convenient applicator delivers cooling relief and calms itching for immediate comfort.", "Itch relief stick", Countries.USA, 0, 5.99),
                ("First Aid Medicine", "BurnJel Burn Relief Gel", "N/A", "BurnJel Burn Relief Gel soothes and cools minor burns and sunburns. Its water-based formula with lidocaine provides instant relief from pain and discomfort.", "Burn relief gel", Countries.USA, 0, 8.99),
                ("First Aid Medicine", "Tweezerman Stainless Steel Tweezers", "N/A", "Tweezerman Stainless Steel Tweezers are precision-crafted for easy and precise splinter removal. Their pointed tips and stainless steel construction ensure durability and effectiveness.", "Tweezers", Countries.USA, 0, 10.99),
                ("First Aid Medicine", "Steri-Strip Skin Closures", "N/A", "Steri-Strip Skin Closures provide non-invasive wound closure for small cuts and incisions. Their breathable material and adhesive backing promote faster healing and reduce scarring.", "Skin closures", Countries.USA, 0, 6.49),
                ("First Aid Medicine", "First Aid Only CPR Mask", "N/A", "First Aid Only CPR Mask provides a barrier between rescuer and victim during CPR. Its compact design and one-way valve ensure safe and effective resuscitation in emergency situations.", "CPR mask", Countries.USA, 0, 4.99),
                ("First Aid Medicine", "Hydrogen Peroxide", "N/A", "Hydrogen Peroxide is a versatile antiseptic solution used for wound cleaning and disinfection. Its bubbling action helps remove debris and kill bacteria for effective wound care.", "Antiseptic solution", Countries.USA, 0, 2.99),
                ("First Aid Medicine", "Tegaderm Transparent Dressings", "N/A", "Tegaderm Transparent Dressings provide a waterproof and breathable barrier to protect wounds and IV sites. Their transparent design allows for easy monitoring of the wound without removing the dressing.", "Transparent dressings", Countries.USA, 0, 9.99)


            };
        }

        public static void FillMedicine(Database db)
        {
            var entries = Init();
            Random random = new Random();
            foreach (var entry in entries)
            {
                db.Catalog.Add(new Medicine
                {
                    Category = entry.Category,
                    Name = entry.Name,
                    ActiveComponent = entry.ActiveComponent,
                    Description = entry.Description,
                    ReleaseForm = entry.ReleaseForm,
                    Distributer = Enum.GetName(typeof(Countries), random.Next(Enum.GetValues(typeof(Countries)).Length)),
                    Expiration = entry.Expiration,
                    Price = entry.Price,
                    Prescription = random.Next(2) == 0 // random boolean value
                }); ;
            }

            db.SaveChanges(); // save changes to database
        }

    }
}
