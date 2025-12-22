Ok Something is still wrong.

BMP image:

Statistics
Original size:  227110 bytes
Header size:    138 bytes (preserved)
Pixel data:     226972 bytes
Codewords:      151315
Error probability:      2.0%
Raw bit errors (no code):       36048
Channel errors (with code):     69984
Errors corrected:       174891
uncorrectable blocks:   0





Input
Text to send:
Hello, World! This is a test of Golay error correction.
Error Probability:
0.05
5%
Send Through Channel
Original Text
Hello, World! This is a test of Golay error correction.
55 bytes, 440 bits
WITHOUT Error Correction
Text sent directly through channel (NO error correction)
Hul�g( Worl�^A Tiiq isa vest of GOh�y0mrror"copreation.
Bit Errors: 20✗ Text corrupted
WITH Golay Error Correction
Text encoded with Golay code (WITH error correction)
Hm�L�W�b,,#��`��`� i`tg�t(��(�od�y��2r�b��o�rek�Kg,^N
Codewords sent: 37
Total bits sent: 851
Bit errors in channel: 44
Errors corrected: 65
Uncorrectable blocks: 0
✓ All errors corrected!

Comparison
Without code matches original: ✗ No
With code matches original: ✗ No
too many errors for Golay to correct
