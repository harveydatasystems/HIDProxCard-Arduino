//This code is designed to work with WEIGAND RFID Devices

#define MAX_BITS 100                 // max number of bits 
#define WAIT_COUNTER  3000      // time to wait for another weigand pulse.  

unsigned char databits[MAX_BITS];   // data buffer
unsigned char bitCount;             // Buffer Size
unsigned char flagDone;             // indicates no more data is incomming. 
unsigned int wait_counter;          // countdown until we assume there are no more bits
unsigned long facilityCode=0;       // rfid card facility
unsigned long cardCode=0;           // rfid card code


void InitRFID()
{
  wait_counter = WAIT_COUNTER;
}

// interrupt that happens when Interrupt 0 goes low (0 bit)
void ISR_INT0() {
  ReceiveBit(0);
}
// interrupt that happens when Interrupt 1 goes low (1 bit)
void ISR_INT1() {
  ReceiveBit(1);
}
void ReceiveBit( char value) {   
  
  if (value == 1)
     databits[bitCount] = 1;

  //Serial.print(value);
  bitCount++;
  flagDone = 0;
  wait_counter = WAIT_COUNTER;    
}
bool CheckForData(unsigned long &facility, unsigned long &card)
{
  facility = 0;
  card = 0;
  
  // Wait to make sure there is no more incomming data
  if (!flagDone) {
    if (--wait_counter == 0)
      flagDone = 1;  
  }

    // if we have bits and we the weigand counter went out
  if (bitCount > 0 && flagDone) {
    unsigned char i;
 
    switch(bitCount){
      case 35: // 35 bit HID Corporate 1000 format - facility code = bits 2 to 14

        for (i=2; i<14; i++) {
         facilityCode <<=1;
         facilityCode |= databits[i];
        }
   
        // card code = bits 15 to 34
        for (i=14; i<34; i++) {
           cardCode <<=1;
           cardCode |= databits[i];
        }

        break;
      case 26: // standard 26 bit format - facility code = bits 2 to 9

        for (i=1; i<9; i++) {
           facilityCode <<=1;
           facilityCode |= databits[i];
        }
   
        // card code = bits 10 to 23
        for (i=9; i<25; i++) {
           cardCode <<=1;
           cardCode |= databits[i];
        }

        break;
    }    
    
    facility = facilityCode;
    card = cardCode;

    // CleanUp
    bitCount = 0;
    facilityCode = 0;
    cardCode = 0;
    for (i=0; i<MAX_BITS; i++) 
      databits[i] = 0;

    return true;
  }
  return false;
}

