#include "main.h"

USB_ClassInfo_CDC_Device_t VirtualSerial_CDC_Interface =
	{
		.Config =
			{
				.ControlInterfaceNumber   = 0,
				.DataINEndpoint           =
					{
						.Address          = CDC_TX_EPADDR,
						.Size             = CDC_TXRX_EPSIZE,
						.Banks            = 1,
					},
				.DataOUTEndpoint =
					{
						.Address          = CDC_RX_EPADDR,
						.Size             = CDC_TXRX_EPSIZE,
						.Banks            = 1,
					},
				.NotificationEndpoint =
					{
						.Address          = CDC_NOTIFICATION_EPADDR,
						.Size             = CDC_NOTIFICATION_EPSIZE,
						.Banks            = 1,
					},
			},
	};

static FILE USBSerialStream;
volatile char data = -1; //-1 no data, 0 data ready, >0 reading data
volatile char right;
volatile char left;
volatile int time;
volatile char strobe_on = 1;
volatile char state = 0;
	
void delay(int ms)
{
	for(int i = 0; i < ms; i++)
		_delay_ms(8);

}
	
int main(void)
{

	SetupHardware();
    
	CDC_Device_CreateStream(&VirtualSerial_CDC_Interface, &USBSerialStream);

	GlobalInterruptEnable();

	for (;;)
	{
		int16_t b = CDC_Device_ReceiveByte(&VirtualSerial_CDC_Interface);
		
		if(b > -1)
		{
			if (b == 0x00) data = 1;
			else if (data == 1)
			{
				right = b;
				data++;
			}
			else if (data == 2)
			{
				left = b;
				data++;
			}
			else if (data == 3)
			{
				time = 0;
				time |= (b<<8);
				data = 4;
			}
			else if (data == 4)
			{
				time |= (b);
				data = 0;
			}
			
		}
		
		CDC_Device_USBTask(&VirtualSerial_CDC_Interface);
		USB_USBTask();
		
		if (data == 0)
		{
			while (!eeprom_is_ready());
			eeprom_write_byte((uint8_t*)0, (uint8_t)right);
			while (!eeprom_is_ready());
			eeprom_write_byte((uint8_t*)8, (uint8_t)left);
			while (!eeprom_is_ready());
			eeprom_write_word(( uint16_t *)16, (uint16_t)time);
			data = -1;
		}
		

		
		if (PIND && (1 << PD2))
		{
			for (int i=0; i<8; i++)
			{
				if(!(PIND && (1 << PD2)))
				break;
				
				if (right & (1 << i)) PORTB &= ~(1<<4);
				else PORTB |= (1<<4);

				
				if (left & (1 << i)) PORTB &= ~(1<<5);
				else PORTB |= (1<<5);
				
				delay(time);//wiksa
			}
		}
		else
			PORTB = 0;
	}
}



void SetupHardware(void)
{
	DDRB |= (1<<4) | (1<<5); //lampy
	DDRD = 0;
	PORTD = 0xff;
	
	MCUSR &= ~(1 << WDRF);
	wdt_disable();
	
	clock_prescale_set(clock_div_1);
	
	while (!eeprom_is_ready());
	
	right = eeprom_read_byte((uint8_t*)0);
	left = eeprom_read_byte((uint8_t*)8);
	time = eeprom_read_word((uint16_t*)16);

	USB_Init();
}

void EVENT_USB_Device_Connect(void)
{
	strobe_on = 0;
}

void EVENT_USB_Device_Disconnect(void)
{
	strobe_on = 1;
}

void EVENT_USB_Device_ConfigurationChanged(void)
{
	bool ConfigSuccess = true;

	ConfigSuccess &= CDC_Device_ConfigureEndpoints(&VirtualSerial_CDC_Interface);

	/*
	if(ConfigSuccess)
	{
		
	}
	else
	{
		
	}
	*/
}

void EVENT_USB_Device_ControlRequest(void)
{
	CDC_Device_ProcessControlRequest(&VirtualSerial_CDC_Interface);
}

