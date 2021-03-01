void TestLED(int duration)
{
  for (int i = 0; i<3; i++)
  {
    SetLED_Red();
    delay(duration);
    SetLED_Orange();
    delay(duration);
    SetLED_Green(); 
    delay(duration);    
  }  
}

void SetLED_Red()
{
  digitalWrite(LED_RED, LOW);  // Red On
  digitalWrite(LED_GREEN, HIGH);  // Green Off
}
void SetLED_Green()
{
  digitalWrite(LED_RED, HIGH);  // Red Off
  digitalWrite(LED_GREEN, LOW);  // Green On
}
void SetLED_Orange()
{
  digitalWrite(LED_RED, LOW);  // Red On
  digitalWrite(LED_GREEN, LOW);  // Green On
}
void SetLED_Off()
{
  digitalWrite(LED_RED, HIGH);  // Red On
  digitalWrite(LED_GREEN, HIGH);  // Green On
}

