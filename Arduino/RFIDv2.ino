
//Setup Pins
int DATA0 = 2;
int DATA1 = 3;
int LED_GREEN = 5;
int LED_RED = 6;
int BEEP_BEEP = 7;

void setup() {
  //Set Pinmodes
  pinMode(LED_RED, OUTPUT);  
  pinMode(LED_GREEN, OUTPUT);  
  pinMode(BEEP_BEEP, OUTPUT);  
  pinMode(DATA0, INPUT);     // DATA0 (INT0)
  pinMode(DATA1, INPUT);     // DATA1 (INT1)    
  //Set Initial Values
  digitalWrite(BEEP_BEEP, HIGH); // High = off
  digitalWrite(LED_RED, HIGH); // High = off
  digitalWrite(LED_GREEN, HIGH); // High = off

  //Bind interupt functions
  attachInterrupt(0, ISR_INT0, FALLING);  
  attachInterrupt(1, ISR_INT1, FALLING);

  //Open Serial Communication
  Serial.begin(9600);

  TestBeep(500);
  TestLED(500);
  InitRFID();
}

void loop() {

  // put your main code here, to run repeatedly:
  bool hasValue = false;
  unsigned long facility = 0;
  unsigned long card = 0;
  
  hasValue = CheckForData(facility, card);  
  if (hasValue)
  {
      SetLED_Orange();
    
      Serial.write(2);
      Serial.print(facility);
      Serial.print(":");
      Serial.print(card);      
      Serial.write(3);      

      delay(500);
      SetLED_Green();
  }


  if (Serial.available() > 0) {
    int incomingByte = Serial.read();
    
    if (incomingByte == 5)  // Keep alive Inquiry
      Serial.write(6);      // Keep alive Ack Response
  }

  
    

}

