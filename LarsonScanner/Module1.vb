' Larson Scanner with 6 LED

' Need 6 LED, 6 resistors into pins 3, 5, 6, 9, 10, 11 (PWMf).

' This example code is in the public domain.

Imports Microsoft.SPOT
Imports Microsoft.SPOT.Hardware
Imports SecretLabs.NETMF.Hardware
Imports SecretLabs.NETMF.Hardware.Netduino

Module Module1

  Private usePWM As Boolean = True

  Private led() As Integer = {3, 5, 6, 9, 10, 11}

  Private brightness As Integer = 255 ' how bright is the LED is
  Private fadeAmount As Integer = 5 ' how many points to fade the LED by

  Private currentLED As Integer = 1
  Private flow As Integer = 1

  Private ledCount As Integer = led.Length
  Private ledPatternSpan As Integer = 865
  Private ledPause As Integer = ledPatternSpan \ ledCount

  Private button As InputPort = Nothing ' Using the Netduino approach, for now, for the button press.

  Private mode As Integer = 0 ' 0 = Basic (Digital), 1 = Basic (PWM), 2 = Cylon, 3 = KITT

  Sub Setup()

    If Not usePWM Then ' Digital
      For i = 0 To 5
        pinMode(led(i), OUTPUT)
      Next
    End If

    ' For now, I'm using the Netduino approach...
    button = New InputPort(Pins.ONBOARD_SW1, False, Port.ResistorMode.Disabled)

    If usePWM Then mode = 1

  End Sub

  Sub [Loop]()

    Dim buttonState = button.Read ' Again, the Netduino approach...
    If buttonState Then
      currentLED = 1 : flow = 1
      If usePWM Then
        mode += 1
        If mode > 3 Then mode = 1
      End If
      Debug.Print("Button pressed; mode = " & mode.ToString)
    End If

    Select Case mode
      Case 0 ' Basic (Digital)
        LoopBasicDigital()
      Case 1 ' Basic (PWM)
        LoopBasicPwm()
      Case 2 ' Cylon
        LoopCylon()
      Case 3 ' KITT
        LoopKITT()
      Case Else
        If usePWM Then
          mode = 1
        Else
          mode = 0
        End If
    End Select

  End Sub

  Private Sub LoopBasicDigital()

    For i = 0 To 5
      digitalWrite(led(i), False)
    Next

    'digitalWrite(led(currentLED), False)

    If currentLED = 5 OrElse currentLED = 0 Then
      flow = -flow
    End If
    currentLED += flow

    digitalWrite(led(currentLED), True)

    delay(ledPause)

  End Sub

  Private Sub LoopBasicPwm()

    For i = 0 To 5
      analogWrite(led(i), 0)
    Next

    analogWrite(led(currentLED), 255)

    If currentLED = 5 OrElse currentLED = 0 Then
      flow = -flow
    End If
    currentLED += flow

    delay(ledPause)

  End Sub

  Private Sub LoopCylon()

    ' Cylon needs to have a faded LED on both sides of the bright ("center") in order 
    ' to mimic an eye.

    ' Turn off all...
    For i = 0 To ledCount - 1
      analogWrite(led(i), 0)
    Next

    'If currentLED - 2 > -1 Then
    '  analogWrite(led(currentLED - 2), 16)
    'End If

    If currentLED - 1 > -1 Then
      analogWrite(led(currentLED - 1), 16)
    End If

    analogWrite(led(currentLED), 255)

    If currentLED + 1 < ledCount Then
      analogWrite(led(currentLED + 1), 16)
    End If

    'If currentLED + 2 < ledCount Then
    '  analogWrite(led(currentLED + 2), 16)
    'End If

    If currentLED = ledCount - 1 OrElse currentLED = 0 Then
      flow = -flow
    End If
    currentLED += flow

    delay(ledPause)

  End Sub

  Private Sub LoopKITT()

    ' KITT will have a bright light followed by trail behind the main.

    ' Turn off all...
    For i = 0 To ledCount - 1
      analogWrite(led(i), 0)
    Next

    If flow > 0 Then

      If currentLED - 2 > -1 Then
        analogWrite(led(currentLED - 2), 16)
      End If

      If currentLED - 1 > -1 Then
        analogWrite(led(currentLED - 1), 50)
      End If

    End If

    analogWrite(led(currentLED), 255)

    If flow < 0 Then

      If currentLED + 1 < ledCount Then
        analogWrite(led(currentLED + 1), 50)
      End If

      If currentLED + 2 < ledCount Then
        analogWrite(led(currentLED + 2), 16)
      End If

    End If

    If currentLED = ledCount - 1 OrElse currentLED = 0 Then
      flow = -flow
    End If
    currentLED += flow

    delay(ledPause)

  End Sub

#Region "Arduino Compatibility Layer"

  'Imports Microsoft.SPOT
  'Imports Microsoft.SPOT.Hardware
  'Imports SecretLabs.NETMF.Hardware
  'Imports SecretLabs.NETMF.Hardware.Netduino
  'Imports SecretLabs.NETMF.Hardware.Netduino.Pins
  'Imports System.Threading.Thread

  Const HIGH = True
  Const LOW = False

  Const LED_BUILTIN = 13
  Const INPUT = 0
  Const INPUT_PULLUP = 3
  Const OUTPUT = 1
  Const PCM = 2

  Private m_inputPins(13) As Microsoft.SPOT.Hardware.InputPort
  Private m_outputPins(13) As Microsoft.SPOT.Hardware.OutputPort
  Private m_outputPwm(13) As Microsoft.SPOT.Hardware.PWM

  Sub Main()

    Setup()

    Do
      [Loop]()
    Loop

  End Sub

  Private Sub reset()

    ' Note: Reset currently DOES NOT WORK!

    For Each pin In m_inputPins
      If pin IsNot Nothing Then
        pin.Dispose()
        pin = Nothing
      End If
    Next
    For Each pin In m_outputPins
      If pin IsNot Nothing Then
        pin.Dispose()
        pin = Nothing
      End If
    Next
    For Each pin In m_outputPwm
      If pin IsNot Nothing Then
        pin.Stop()
        pin.Dispose()
        pin = Nothing
      End If
    Next
  End Sub

  Private Sub delay(milliseconds As Integer)
    Thread.Sleep(milliseconds)
  End Sub

  Private Sub pinMode(pin As Integer, value As Integer)
    Dim translated As Microsoft.SPOT.Hardware.Cpu.Pin
    Select Case pin
      Case 0 : translated = SecretLabs.NETMF.Hardware.Netduino.Pins.GPIO_PIN_D0
      Case 1 : translated = SecretLabs.NETMF.Hardware.Netduino.Pins.GPIO_PIN_D1
      Case 2 : translated = SecretLabs.NETMF.Hardware.Netduino.Pins.GPIO_PIN_D2
      Case 3 : translated = SecretLabs.NETMF.Hardware.Netduino.Pins.GPIO_PIN_D3
      Case 4 : translated = SecretLabs.NETMF.Hardware.Netduino.Pins.GPIO_PIN_D4
      Case 5 : translated = SecretLabs.NETMF.Hardware.Netduino.Pins.GPIO_PIN_D5
      Case 6 : translated = SecretLabs.NETMF.Hardware.Netduino.Pins.GPIO_PIN_D6
      Case 7 : translated = SecretLabs.NETMF.Hardware.Netduino.Pins.GPIO_PIN_D7
      Case 8 : translated = SecretLabs.NETMF.Hardware.Netduino.Pins.GPIO_PIN_D8
      Case 9 : translated = SecretLabs.NETMF.Hardware.Netduino.Pins.GPIO_PIN_D9
      Case 10 : translated = SecretLabs.NETMF.Hardware.Netduino.Pins.GPIO_PIN_D10
      Case 11 : translated = SecretLabs.NETMF.Hardware.Netduino.Pins.GPIO_PIN_D11
      Case 12 : translated = SecretLabs.NETMF.Hardware.Netduino.Pins.GPIO_PIN_D12
      Case 13 : translated = SecretLabs.NETMF.Hardware.Netduino.Pins.GPIO_PIN_D13 'ONBOARD_LED
      Case Else
        Stop 'translated = SecretLabs.NETMF.Hardware.Netduino.Pins.GPIO_NONE
    End Select
    Select Case value
      Case INPUT_PULLUP
        m_inputPins(pin - 1) = New Microsoft.SPOT.Hardware.InputPort(translated, False, Microsoft.SPOT.Hardware.Port.ResistorMode.PullUp)
      Case INPUT
        m_inputPins(pin - 1) = New Microsoft.SPOT.Hardware.InputPort(translated, False, Microsoft.SPOT.Hardware.Port.ResistorMode.PullDown)
      Case OUTPUT
        m_outputPins(pin - 1) = New Microsoft.SPOT.Hardware.OutputPort(translated, False)
      Case PCM
        m_outputPins(pin - 1) = New Microsoft.SPOT.Hardware.OutputPort(translated, False)
      Case Else
        Stop
    End Select

  End Sub

  Private Sub digitalWrite(pin As Integer, value As Boolean)
    m_outputPins(pin - 1).Write(value)
  End Sub

  Private Sub analogWrite(pin As Integer, value As Integer)

    Try

      'pin: the pin to write to. 
      'value: the duty cycle: between 0 (always off) and 255 (always on). 
      '490 Hz

      Dim dutyCycle As Double
      If value = 0 Then
        dutyCycle = 0
      ElseIf value = 255 Then
        dutyCycle = 1
      ElseIf value > 0 AndAlso value < 255 Then
        dutyCycle = value / 255
      Else
        dutyCycle = -1
      End If

      Dim translated As Cpu.PWMChannel
      Select Case pin
        Case 3 : translated = PWMChannels.PWM_PIN_D3
        Case 5 : translated = PWMChannels.PWM_PIN_D5
        Case 6 : translated = PWMChannels.PWM_PIN_D6
        Case 9 : translated = PWMChannels.PWM_PIN_D9
        Case 10 : translated = PWMChannels.PWM_PIN_D10
        Case 11 : translated = PWMChannels.PWM_PIN_D11
        Case Else
          pin = Cpu.PWMChannel.PWM_NONE 'PWMChannels.PWM_NONE
      End Select

      If pin <> PWMChannels.PWM_NONE AndAlso
         dutyCycle > -1 Then
        If m_outputPwm(pin - 1) Is Nothing Then
          m_outputPwm(pin - 1) = New Microsoft.SPOT.Hardware.PWM(translated, 490, dutyCycle, False)
          m_outputPwm(pin - 1).Start()
        Else
          m_outputPwm(pin - 1).Stop()
          m_outputPwm(pin - 1).DutyCycle = dutyCycle
          m_outputPwm(pin - 1).Start()
        End If
      End If

    Catch ex As Exception
      Debug.Print("ERROR: " & ex.Message)
    End Try

  End Sub

#End Region

End Module
