namespace com.upokecenter.util {
using System;

// C# version of DateTimeUtility
  
public sealed class DateTimeUtility {
	private DateTimeUtility(){}

		
		public static int convertYear(int twoDigitYear){
			DateTime dt=DateTime.UtcNow;
			int thisyear=dt.Year;
			int this2digityear=thisyear%100;
			int actualyear=twoDigitYear+(thisyear-this2digityear);
			if(twoDigitYear-this2digityear>50){
				actualyear-=100;
			}
			return actualyear;
		}
		
		private static int[] normalDays = { 0, 31, 28, 31, 30, 31, 30, 31, 31, 30,
			31, 30, 31 };
		private static int[] leapDays = { 0, 31, 29, 31, 30, 31, 30, 31, 31, 30,
			31, 30, 31 };
		private static int[] year1582Days = { 0, 31, 28, 31, 30, 31, 30, 31, 31,
			30, 21, 30, 31 };
		private static int[] normalToMonth = { 0, 0x1f, 0x3b, 90, 120, 0x97, 0xb5,
			0xd4, 0xf3, 0x111, 0x130, 0x14e, 0x16d };
		private static int[] leapToMonth = { 0, 0x1f, 60, 0x5b, 0x79, 0x98, 0xb6,
			0xd5, 0xf4, 0x112, 0x131, 0x14f, 0x16e };
		private static int[] year1582ToMonth = { 0, 0x1f, 0x3b, 90, 120, 0x97,
			0xb5, 0xd4, 0xf3, 0x111, 294, 324, 355 };
		
		private static bool isValidDay(int year, int month, int day){
			if (year == 1582) {
				if(month==10 && day>4 && day<15)return false;
				return day<=normalDays[month];
			} else if ((year & 3) != 0
			           || (year > 1582 && year % 100 == 0 && year % 400 != 0)) {
				return day<=normalDays[month];
			} else {
				return day<=leapDays[month];
			}
		}
		
		private static long GetNumberOfDaysGregorian(int year, int month, int day) {
			long numDays = 0;
			int startYear = 1970;
			if (year < startYear) {
				for (int i = startYear - 1; i > year; i--) {
					if (i == 1582) {
						numDays -= 355; // not a leap year, 10 days missing
					} else if ((i & 3) != 0
					           || (i > 1582 && i % 100 == 0 && i % 400 != 0)) {
						numDays -= 365;
					} else {
						numDays -= 366;
					}
				}
				if (year == 1582) {
					numDays -= 355 - year1582ToMonth[month];
					numDays -= year1582Days[month] - day + 1;
					if (month == 10 && day >= 15)
						numDays -= 10;
				} else if ((year & 3) != 0
				           || (year > 1582 && year % 100 == 0 && year % 400 != 0)) {
					numDays -= 365 - normalToMonth[month];
					numDays -= normalDays[month] - day + 1;
				} else {
					numDays -= 366 - leapToMonth[month];
					numDays -= leapDays[month] - day + 1;
				}
			} else {
				bool isNormalYear = (year & 3) != 0
					|| (year % 100 == 0 && year % 400 != 0);
				int i = startYear;
				for (; i + 401 < year; i += 400) {
					numDays += 146097;
				}
				for (; i < year; i++) {
					if ((i & 3) != 0 || (i % 100 == 0 && i % 400 != 0)) {
						numDays += 365;
					} else {
						numDays += 366;
					}
				}
				/**/
				day -= 1;
				if (isNormalYear) {
					numDays += normalToMonth[month - 1];
				} else {
					numDays += leapToMonth[month - 1];
				}
				numDays += day;
			}
			return numDays;
		}
		
		private static int FloorDiv(int a, int n) {
			return a >= 0 ? a / n : -1 - (-1 - a) / n;
		}

		private static long FloorDiv(long a, long n) {
			return a >= 0 ? a / n : -1 - (-1 - a) / n;
		}

		private static long FloorMod(long a, long n) {
			return a-FloorDiv(a,n)*n;
		}


		private static void GetNormalizedPartGregorian(int year, // year 1 is equal to 1
		                                              int month, // January is equal to 1
		                                              long day, // first day of month is equal to 1
		                                              int[] dest) {
			int divResult;
			divResult = FloorDiv((month - 1), 12);
			year += divResult;
			month = ((month - 1) - 12 * divResult) + 1;
			int[] dayArray = ((year & 3) != 0 || (year > 1582 && year % 100 == 0 && year % 400 != 0)) ? normalDays
				: leapDays;
			if (day > 101 && year > 1582) {
				long count = (day - 100) / 146097;
				day -= count * 146097;
				year = (int)(year+count * 400);
			}
			while (true) {
				while (day < -146200 && year < 1582) {
					day += 146100;
					year -= 400;
				}
				int days = (year == 1582 && month == 10) ? 21 : dayArray[month];
				if (day > 0 && day <= days) {
					break;
				}
				if (day > days) {
					day -= days;
					if (month == 12) {
						month = 1;
						year++;
						dayArray = ((year & 3) != 0 || (year > 1582
						                                && year % 100 == 0 && year % 400 != 0)) ? normalDays
							: leapDays;
					} else {
						month++;
					}
				}
				if (day <= 0) {
					divResult = FloorDiv((month - 2), 12);
					year += divResult;
					month = ((month - 2) - 12 * divResult) + 1;
					dayArray = ((year & 3) != 0 || (year > 1582 && year % 100 == 0 && year % 400 != 0)) ? normalDays
						: leapDays;
					day += (year == 1582 && month == 10) ? 21 : dayArray[month];
				}
			}
			dest[0]=year;
			dest[1]=month;
			if (month == 10 && year == 1582 && day >= 5)
				dest[2]=(int)(day + 10);
			else
				dest[2]=(int)day;
		}
		
		public static int[] getGmtDateComponents(long date){
			long days = FloorDiv(date, 86400000L) + 1;
			int[] ret=new int[9];
			GetNormalizedPartGregorian(1970,1,days,ret);
			ret[3]=(int)(FloorMod(date, 86400000L) / 3600000L);
			ret[4]=(int)(FloorMod(date, 3600000L) / 60000L);
			ret[5]=(int)(FloorMod(date, 60000L) / 1000L);
			ret[6]=(int)FloorMod(date, 1000L);
			// day of week: 1 is Sunday, 2 is Monday, and so on
			ret[7]=(int)(FloorMod(days+3,7)+1);
			ret[8]=0;
			return ret;
		}

		public static int[] getLocalDateComponents(long date){
			long days = FloorDiv(date, 86400000L) + 1;
			int[] ret=new int[9];
			// Separate GMT date into components
			GetNormalizedPartGregorian(1970,1,days,ret);
			ret[3]=(int)(FloorMod(date, 86400000L) / 3600000L);
			ret[4]=(int)(FloorMod(date, 3600000L) / 60000L);
			ret[5]=(int)(FloorMod(date, 60000L) / 1000L);
			TimeZone tz=TimeZone.CurrentTimeZone;
			DateTime dt=tz.ToLocalTime(new DateTime(ret[0],ret[1],ret[2],ret[3],ret[4],ret[5]));
			ret[0]=dt.Year;
			ret[1]=dt.Month;
			ret[2]=dt.Day;
			ret[3]=dt.Hour;
			ret[4]=dt.Minute;
			ret[5]=dt.Second;
			ret[6]=dt.Millisecond;
			// time zone offset
			ret[8]=(int)Math.Round(tz.GetUtcOffset(dt).TotalMinutes);
			DayOfWeek dow=dt.DayOfWeek;
			if(dow== DayOfWeek.Sunday)ret[7]=1;
			else if(dow== DayOfWeek.Monday)ret[7]=2;
			else if(dow== DayOfWeek.Tuesday)ret[7]=3;
			else if(dow== DayOfWeek.Wednesday)ret[7]=4;
			else if(dow== DayOfWeek.Thursday)ret[7]=5;
			else if(dow== DayOfWeek.Friday)ret[7]=6;
			else if(dow== DayOfWeek.Saturday)ret[7]=7;
			else ret[7]=0;
			return ret;
		}

		public static int[] getCurrentGmtDateComponents(){
			DateTime dt=DateTime.UtcNow;
			int[] ret=new int[8];
			ret[0]=dt.Year;
			ret[1]=dt.Month;
			ret[2]=dt.Day;
			ret[3]=dt.Hour;
			ret[4]=dt.Minute;
			ret[5]=dt.Second;
			ret[6]=dt.Millisecond;
			ret[8]=0; // time zone offset is 0 for GMT
			DayOfWeek dow=dt.DayOfWeek;
			if(dow== DayOfWeek.Sunday)ret[7]=1;
			else if(dow== DayOfWeek.Monday)ret[7]=2;
			else if(dow== DayOfWeek.Tuesday)ret[7]=3;
			else if(dow== DayOfWeek.Wednesday)ret[7]=4;
			else if(dow== DayOfWeek.Thursday)ret[7]=5;
			else if(dow== DayOfWeek.Friday)ret[7]=6;
			else if(dow== DayOfWeek.Saturday)ret[7]=7;
			else ret[7]=0;
			return ret;
		}

		public static int[] getCurrentLocalDateComponents(){
			DateTime dt=DateTime.Now;
			int[] ret=new int[9];
			ret[0]=dt.Year;
			ret[1]=dt.Month;
			ret[2]=dt.Day;
			ret[3]=dt.Hour;
			ret[4]=dt.Minute;
			ret[5]=dt.Second;
			ret[6]=dt.Millisecond;
			// time zone offset
			ret[8]=(int)Math.Round(TimeZone.CurrentTimeZone.GetUtcOffset(dt).TotalMinutes);
			DayOfWeek dow=dt.DayOfWeek;
			if(dow== DayOfWeek.Sunday)ret[7]=1;
			else if(dow== DayOfWeek.Monday)ret[7]=2;
			else if(dow== DayOfWeek.Tuesday)ret[7]=3;
			else if(dow== DayOfWeek.Wednesday)ret[7]=4;
			else if(dow== DayOfWeek.Thursday)ret[7]=5;
			else if(dow== DayOfWeek.Friday)ret[7]=6;
			else if(dow== DayOfWeek.Saturday)ret[7]=7;
			else ret[7]=0;
			return ret;
		}
	
		public static String toXmlSchemaDate(int[] components){
		System.Text.StringBuilder b=new System.Text.StringBuilder();
		// Date
		b.Append(String.Format(System.Globalization.CultureInfo.InvariantCulture,
				"{0:d4}-{1:d2}-{2:d2}T",components[0],components[1],components[2]));
		// Time
		if(components[3]!=0 ||
				components[4]!=0 ||
				components[5]!=0 ||
				components[6]!=0){
			b.Append(String.Format(System.Globalization.CultureInfo.InvariantCulture,
					"{0:d2}:{1:d2}:{2:d2}",components[3],components[4],components[5]));
			// Milliseconds
			if(components[6]!=0){
				b.Append(String.Format(System.Globalization.CultureInfo.InvariantCulture,".{0:d3}",components[6]));
			}
		}
		// Time zone offset
		if(components[8]==0){
			b.Append('Z');
		} else {
			int tzabs=Math.Abs(components[8]);
			b.Append(components[8]<0 ? '-' : '+');
			b.Append(String.Format(System.Globalization.CultureInfo.InvariantCulture,
					"{0:d2}:{1:d2}",tzabs/60,tzabs%60));
		}
		return b.ToString();
	}
	public static String toXmlSchemaGmtDate(long time){
		return toXmlSchemaDate(getGmtDateComponents(time));
	}
	public static String toXmlSchemaLocalDate(long time){
		return toXmlSchemaDate(getLocalDateComponents(time));
	}

	
		public static long toGmtDate(int year, int month, int day,
		                          int hour, int minute, int second){
			long days;
			if(month<1||month>12||day<1||day>31||hour<0||hour>23||
			   minute<0||minute>59||second<0||second>59)
				throw new ArgumentException();
			if(!isValidDay(year,month,day))
				throw new ArgumentException();
			days = GetNumberOfDaysGregorian(year, month, day);
			long ticks = days * 86400000L;
			int hms = 3600 * hour + 60 * minute + second;
			ticks += hms * 1000L;
			return ticks;
		}
		
		public static long getCurrentDate(){
			DateTime t=DateTime.UtcNow;
			long msec=t.Millisecond;
			long time=toGmtDate(t.Year,t.Month,t.Day,t.Hour,t.Minute,t.Second)+msec;
			return time;
		}
}

}
