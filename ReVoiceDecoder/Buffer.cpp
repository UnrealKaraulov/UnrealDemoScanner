#include "Buffer.hpp"
#include <iomanip> // byteStr()

/************************* WRITING *************************/

Buffer::Buffer()  {
}
Buffer::Buffer(const std::vector<unsigned char> &_buffer) :
    buffer(_buffer) {
}

void Buffer::setBuffer(std::vector<unsigned char> &_buffer)  {
    buffer = _buffer;
}
const std::vector<unsigned char> &Buffer::getBuffer() const  {
    return buffer;
}
void Buffer::clear()  {
    buffer.clear();
    readOffset = 0;
    writeOffset = 0;
}

std::string Buffer::byteStr(bool LE) const  {
    std::stringstream byteStr;
    byteStr << std::hex << std::setfill('0');

    if (LE == true) {
        for (unsigned long long i = 0; i < buffer.size(); ++i)
            byteStr << std::setw(2) << (unsigned short)buffer[i] << " ";
    } else {
        unsigned long long size = buffer.size();
        for (unsigned long long i = 0; i < size; ++i)
            byteStr << std::setw(2) << (unsigned short)buffer[size - i - 1] << " ";
    }

    return byteStr.str();
}

template <class T> inline void Buffer::writeBytes(const T &val, bool LE) {
    unsigned int size = sizeof(T);

    if (LE == true) {
        for (unsigned int i = 0, mask = 0; i < size; ++i, mask += 8)
            buffer.push_back(val >> mask);
    } else {
        unsigned const char *array = reinterpret_cast<unsigned const char*>(&val);
        for (unsigned int i = 0; i < size; ++i)
            buffer.push_back(array[size - i - 1]);
    }
    writeOffset += size;
}

unsigned long long Buffer::getWriteOffset() const  {
    return writeOffset;
}

void Buffer::writeBool(bool val)  {
    writeBytes<bool>(val);
}
void Buffer::writeStr(const std::string &str)  {
    for (const unsigned char &s : str) writeInt8(s);
}
void Buffer::writeInt8(char val)  {
    writeBytes<char>(val);
}
void Buffer::writeUInt8(unsigned char val)  {
    writeBytes<unsigned char>(val);
}

void Buffer::writeInt16_LE(short val)  {
    writeBytes<short>(val);
}
void Buffer::writeInt16_BE(short val)  {
    writeBytes<short>(val, false);
}
void Buffer::writeUInt16_LE(unsigned short val)  {
    writeBytes<unsigned short>(val);
}
void Buffer::writeUInt16_BE(unsigned short val)  {
    writeBytes<unsigned short>(val, false);
}

void Buffer::writeInt32_LE(int val)  {
    writeBytes<int>(val);
}
void Buffer::writeInt32_BE(int val)  {
    writeBytes<int>(val, false);
}
void Buffer::writeUInt32_LE(unsigned int val)  {
    writeBytes<unsigned int>(val);
}
void Buffer::writeUInt32_BE(unsigned int val)  {
    writeBytes<unsigned int>(val, false);
}

void Buffer::writeFloat_LE(float val)  {
    union { float fnum; unsigned inum; } u;
    u.fnum = val;
    writeUInt32_LE(u.inum);
}
void Buffer::writeFloat_BE(float val)  {
    union { float fnum; unsigned inum; } u;
    u.fnum = val;
    writeUInt32_BE(u.inum);
}

/************************* READING *************************/

void Buffer::setReadOffset(unsigned long long newOffset)  {
    readOffset = newOffset;
}
unsigned long long Buffer::getReadOffset() const  {
    return readOffset;
}
template <class T> inline T Buffer::readBytes(bool LE) {
    T result = 0;
    unsigned int size = sizeof(T);

    // Do not overflow
    if (readOffset + size > buffer.size())
        return result;

    char *dst = (char*)&result;
    char *src = (char*)&buffer[readOffset];

    if (LE == true) {
        for (unsigned int i = 0; i < size; ++i)
            dst[i] = src[i];
    } else {
        for (unsigned int i = 0; i < size; ++i)
            dst[i] = src[size - i - 1];
    }
    readOffset += size;
    return result;
}

bool Buffer::readBool()  {
    return readBytes<bool>();
}
std::string Buffer::readStr(unsigned long long len)  {
    if (readOffset + len > buffer.size())
        return "Buffer out of range (provided length greater than buffer size)";
    std::string result(buffer.begin() + readOffset, buffer.begin() + readOffset + len);
    readOffset += len;
    return result;
}
std::string Buffer::readStr()  {
    return readStr(buffer.size() - readOffset);
}
char Buffer::readInt8()  {
    return readBytes<char>();
}
unsigned char Buffer::readUInt8()  {
    return readBytes<unsigned char>();
}

short Buffer::readInt16_LE()  {
    return readBytes<short>();
}
short Buffer::readInt16_BE()  {
    return readBytes<short>(false);
}
unsigned short Buffer::readUInt16_LE()  {
    return readBytes<unsigned short>();
}
unsigned short Buffer::readUInt16_BE()  {
    return readBytes<unsigned short>(false);
}

int Buffer::readInt32_LE()  {
    return readBytes<int>();
}
int Buffer::readInt32_BE()  {
    return readBytes<int>(false);
}
unsigned int Buffer::readUInt32_LE()  {
    return readBytes<unsigned int>();
}
unsigned int Buffer::readUInt32_BE()  {
    return readBytes<unsigned int>(false);
}

float Buffer::readFloat_LE()  {
    return readBytes<float>();
}
float Buffer::readFloat_BE()  {
    return readBytes<float>(false);
}
double Buffer::readDouble_LE()  {
    return readBytes<double>();
}
double Buffer::readDouble_BE()  {
    return readBytes<double>(false);
}

Buffer::~Buffer() {
    clear();
}
