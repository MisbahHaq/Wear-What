import { DayWeather, WeatherCondition } from '../types';

export const WEATHER_PRESETS: { label: string; tempC: number; condition: WeatherCondition; icon: string }[] = [
  { label: 'Sunny & Hot (30°C)', tempC: 30, condition: 'hot', icon: '☀️' },
  { label: 'Pleasant & Mild (22°C)', tempC: 22, condition: 'sunny', icon: '🌤️' },
  { label: 'Breezy & Cool (17°C)', tempC: 17, condition: 'windy', icon: '🍃' },
  { label: 'Rainy & Chilly (13°C)', tempC: 13, condition: 'rainy', icon: '🌧️' },
  { label: 'Cold & Crisp (6°C)', tempC: 6, condition: 'chilly', icon: '❄️' },
];

export const DEFAULT_WEEK_WEATHER: DayWeather[] = [
  { city: 'Karachi, Pakistan', tempC: 32, condition: 'hot', label: 'Hot & Dry', icon: '☀️', rainChance: 0 },
  { city: 'Karachi, Pakistan', tempC: 33, condition: 'hot', label: 'Scorching Sun', icon: '🔥', rainChance: 5 },
  { city: 'Karachi, Pakistan', tempC: 31, condition: 'sunny', label: 'Bright & Warm', icon: '🌤️', rainChance: 10 },
  { city: 'Karachi, Pakistan', tempC: 34, condition: 'hot', label: 'Intense Heat', icon: '🔥', rainChance: 0 },
  { city: 'Karachi, Pakistan', tempC: 30, condition: 'sunny', label: 'Clear Afternoon', icon: '☀️', rainChance: 0 },
  { city: 'Karachi, Pakistan', tempC: 35, condition: 'hot', label: 'Peak Summer', icon: '🌡️', rainChance: 15 },
  { city: 'Karachi, Pakistan', tempC: 32, condition: 'sunny', label: 'Warm Breeze', icon: '🌤️', rainChance: 5 },
];

export async function fetchCityWeather(cityName: string): Promise<DayWeather[]> {
  try {
    const geoRes = await fetch(`https://geocoding-api.open-meteo.com/v1/search?name=${encodeURIComponent(cityName)}&count=1&language=en&format=json`);
    if (!geoRes.ok) throw new Error('Geocoding failed');
    const geoData = await geoRes.json();
    
    if (!geoData.results || geoData.results.length === 0) {
      throw new Error('City not found');
    }

    const { latitude, longitude, name, country } = geoData.results[0];

    const weatherRes = await fetch(
      `https://api.open-meteo.com/v1/forecast?latitude=${latitude}&longitude=${longitude}&daily=weathercode,temperature_2m_max,precipitation_probability_max&timezone=auto`
    );
    if (!weatherRes.ok) throw new Error('Weather forecast failed');
    const weatherData = await weatherRes.json();

    const daily = weatherData.daily;
    const result: DayWeather[] = [];

    for (let i = 0; i < 7; i++) {
      const code = daily.weathercode?.[i] ?? 0;
      const maxTemp = Math.round(daily.temperature_2m_max?.[i] ?? 22);
      const rainProb = daily.precipitation_probability_max?.[i] ?? 10;
      
      let condition: WeatherCondition = 'sunny';
      let icon = '☀️';
      let label = 'Sunny & Clear';

      if (code >= 51 && code <= 67) {
        condition = 'rainy';
        icon = '🌧️';
        label = 'Rainy Showers';
      } else if (code >= 71 && code <= 86) {
        condition = 'chilly';
        icon = '❄️';
        label = 'Freezing / Snow';
      } else if (code >= 1 && code <= 3) {
        condition = 'cloudy';
        icon = '⛅';
        label = 'Partly Cloudy';
      } else if (maxTemp >= 28) {
        condition = 'hot';
        icon = '🔥';
        label = 'Hot & Sunny';
      } else if (maxTemp <= 12) {
        condition = 'chilly';
        icon = '🧥';
        label = 'Chilly & Brisk';
      }

      result.push({
        city: `${name}, ${country || ''}`,
        tempC: maxTemp,
        condition,
        label,
        icon,
        rainChance: rainProb
      });
    }

    return result;
  } catch (err) {
    console.warn('Using simulated weather for', cityName, err);
    // Return friendly simulated forecast based on city query
    return DEFAULT_WEEK_WEATHER.map((w, idx) => ({
      ...w,
      city: cityName,
      tempC: Math.max(10, Math.min(32, w.tempC + ((idx % 3) - 1) * 2))
    }));
  }
}
