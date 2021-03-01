void TestBeep(int duration)
{
  for (int i=0; i<4; i++)
  {
    Beep(duration);
    delay(duration);    
  }
}
void Beep(int duration)
{
  digitalWrite(BEEP_BEEP, LOW);
  delay(duration);
  digitalWrite(BEEP_BEEP, HIGH);
}


